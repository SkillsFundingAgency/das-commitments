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
    public async Task ReplayFromFile(ReplayInputFile replayInputFile)
    {
        var cohortIds = ParseCohortIds(replayInputFile.Content);
        if (cohortIds.Count == 0)
        {
            logger.LogWarning("Replay file {FileName} did not contain any valid cohort ids.", replayInputFile.FullPath);
            return;
        }

        var creationDate = DateTime.UtcNow;
        var events = await dbContext.Apprenticeships
            .Where(apprenticeship => cohortIds.Contains(apprenticeship.CommitmentId))
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
            "Replay file {FileName} produced {Count} ApprenticeshipCreatedEvents from {CohortCount} cohort ids.",
            replayInputFile.FullPath, events.Count, cohortIds.Count);

        var dryRun = configuration.ReplayApprenticeshipCreatedEvents?.DryRun ?? true;
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

    public static HashSet<long> ParseCohortIds(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var separators = new[] { ',', '\n', '\r', ';', '\t', ' ' };
        var values = content.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return values
            .Select(NormaliseToken)
            .Select(value => long.TryParse(value, out var id) ? id : (long?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();
    }

    private static string NormaliseToken(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? value : value.Trim().Trim('"', '\'');
    }
}
