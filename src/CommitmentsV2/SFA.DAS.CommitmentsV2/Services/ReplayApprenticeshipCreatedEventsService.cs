using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Services;

public class ReplayApprenticeshipCreatedEventsService(
    ProviderCommitmentsDbContext dbContext,
    CommitmentsV2Configuration configuration,
    IEncodingService encodingService,
    IEventPublisher eventPublisher,
    ILogger<ReplayApprenticeshipCreatedEventsService> logger)
    : IReplayApprenticeshipCreatedEventsService
{
    private const string ApprenticeshipIdHeader = "apprenticeshipid";

    public async Task ReplayFromFile(ReplayInputFile replayInputFile)
    {
        var apprenticeshipIds = ParseApprenticeshipIds(replayInputFile.Content);
        if (apprenticeshipIds.Count == 0)
        {
            logger.LogWarning(
                "Replay file {FileName} did not contain any valid apprenticeship ids. Ensure header is ApprenticeshipId.",
                replayInputFile.FullPath);
            return;
        }

        var creationDate = DateTime.UtcNow;
        var events = await dbContext.Apprenticeships
            .Where(apprenticeship => apprenticeshipIds.Contains(apprenticeship.Id))
            .Join(dbContext.Standards,
                apprenticeship => apprenticeship.StandardUId,
                standard => standard.StandardUId,
                (apprenticeship, standard) => new { apprenticeship, standard })
            .Select(x => new ApprenticeshipCreatedEvent
            {
                ApprenticeshipId = x.apprenticeship.Id,
                CreatedOn = creationDate,
                AgreedOn = x.apprenticeship.Cohort.EmployerAndProviderApprovedOn.GetValueOrDefault(),
                AccountId = x.apprenticeship.Cohort.EmployerAccountId,
                AccountLegalEntityPublicHashedId = x.apprenticeship.Cohort.AccountLegalEntity.PublicHashedId,
                AccountLegalEntityId = x.apprenticeship.Cohort.AccountLegalEntity.Id,
                LegalEntityName = x.apprenticeship.Cohort.AccountLegalEntity.Name,
                ProviderId = x.apprenticeship.Cohort.ProviderId,
                TransferSenderId = x.apprenticeship.Cohort.TransferSenderId,
                ApprenticeshipEmployerTypeOnApproval = x.apprenticeship.Cohort.ApprenticeshipEmployerTypeOnApproval,
                Uln = x.apprenticeship.Uln,
                DeliveryModel = x.apprenticeship.DeliveryModel ?? SFA.DAS.CommitmentsV2.Types.DeliveryModel.Regular,
                TrainingType = x.apprenticeship.ProgrammeType.Value,
                TrainingCode = x.apprenticeship.CourseCode,
                StandardUId = x.apprenticeship.StandardUId,
                TrainingCourseOption = x.apprenticeship.TrainingCourseOption,
                TrainingCourseVersion = x.apprenticeship.TrainingCourseVersion,
                StartDate = x.apprenticeship.StartDate.GetValueOrDefault(),
                EndDate = x.apprenticeship.EndDate.GetValueOrDefault(),
                PriceEpisodes = x.apprenticeship.PriceHistory
                    .Select(p => new PriceEpisode
                    {
                        FromDate = p.FromDate,
                        ToDate = p.ToDate,
                        Cost = p.Cost,
                        EndPointAssessmentPrice = p.AssessmentPrice,
                        TrainingPrice = p.TrainingPrice
                    })
                    .ToArray(),
                ContinuationOfId = x.apprenticeship.ContinuationOfId,
                DateOfBirth = x.apprenticeship.DateOfBirth.GetValueOrDefault(),
                ActualStartDate = x.apprenticeship.ActualStartDate,
                FirstName = x.apprenticeship.FirstName,
                LastName = x.apprenticeship.LastName,
                ApprenticeshipHashedId = encodingService.Encode(x.apprenticeship.Id, EncodingType.ApprenticeshipId),
                LearnerDataId = x.apprenticeship.LearnerDataId,
                LearningType = Enum.Parse<LearningType>(x.standard.ApprenticeshipType, true)
            })
            .ToListAsync();

        logger.LogInformation(
            "Replay file {FileName} produced {Count} ApprenticeshipCreatedEvents from {InputCount} apprenticeship ids.",
            replayInputFile.FullPath, events.Count, apprenticeshipIds.Count);

        var dryRun = configuration.ReplayApprenticeshipCreatedEventsDryRun;
        foreach (var apprenticeshipCreatedEvent in events)
        {
            if (dryRun)
            {
                logger.LogInformation(
                    "DryRun enabled. Would publish ApprenticeshipCreatedEvent for ApprenticeshipId {ApprenticeshipId}.",
                    apprenticeshipCreatedEvent.ApprenticeshipId);
                continue;
            }

            await eventPublisher.Publish(apprenticeshipCreatedEvent);
            logger.LogInformation(
                "Published ApprenticeshipCreatedEvent for ApprenticeshipId {ApprenticeshipId}.",
                apprenticeshipCreatedEvent.ApprenticeshipId);
        }
    }

    public static HashSet<long> ParseApprenticeshipIds(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var separators = new[] { ',', '\n', '\r', ';', '\t', ' ' };
        var values = content.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (values.Length == 0)
        {
            return [];
        }

        return values
            .Select(NormaliseToken)
            .Where(value => !IsHeader(value, ApprenticeshipIdHeader))
            .Select(value => long.TryParse(value, out var id) ? id : (long?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();
    }

    private static string NormaliseToken(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? value : value.Trim().Trim('"', '\'');
    }

    private static bool IsHeader(string value, string expectedHeader)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               string.Equals(NormaliseToken(value), expectedHeader, StringComparison.OrdinalIgnoreCase);
    }

}
