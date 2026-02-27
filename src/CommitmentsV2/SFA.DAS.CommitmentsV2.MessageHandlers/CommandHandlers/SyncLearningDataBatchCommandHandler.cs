using System.Diagnostics;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading;
using LearningType = SFA.DAS.Common.Domain.Types.LearningType;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class SyncLearningDataBatchCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<SyncLearningDataBatchCommandHandler> logger) : IHandleMessages<SyncLearningDataBatchCommand>
{
    public async Task Handle(SyncLearningDataBatchCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("SyncLearningDataBatchCommandHandler invoked for batch {BatchNumber} of {Count}", message.BatchNumber, message.Ids.Count());

        var stopwatch = Stopwatch.StartNew();

        foreach (var apprenticeshipId in message.Ids)
        {
            try
            {
                logger.LogInformation("Getting apprenticeship id {ApprenticeshipId}", apprenticeshipId);

                var apprenticeship =
                    await dbContext.Value.GetApprenticeshipAggregateWithNoTracking(apprenticeshipId, CancellationToken.None);

                var learningSyncEvent = new SyncLearningCommand(CreateEventFromApprenticeship(apprenticeship));

                logger.LogInformation("Sending SyncLearningCommand for Apprenticeship Id {ApprenticeshipId}", apprenticeshipId);
                //await context.Send(learningSyncEvent); //disabled for early testing
            }
            catch (BadRequestException ex)
            {
                logger.LogError(ex, "Error occurred processing Apprenticeship Id {ApprenticeshipId}", apprenticeshipId);
            }
        }

        stopwatch.Stop();
        logger.LogInformation($"SyncLearningDataBatchCommandHandler for batch {message.BatchNumber} completed in {stopwatch.ElapsedMilliseconds}ms");

    }

    public static ApprenticeshipCreatedEvent CreateEventFromApprenticeship(Apprenticeship apprenticeship)
    {
        return new ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = apprenticeship.Id,
            CreatedOn = DateTime.UtcNow, // not used
            AgreedOn = apprenticeship.Cohort.EmployerAndProviderApprovedOn.Value,
            AccountId = apprenticeship.Cohort.EmployerAccountId,
            AccountLegalEntityPublicHashedId = apprenticeship.Cohort.AccountLegalEntity.PublicHashedId,
            AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntity.Id,
            LegalEntityName = apprenticeship.Cohort.AccountLegalEntity.Name,
            ProviderId = apprenticeship.Cohort.ProviderId,
            TransferSenderId = apprenticeship.Cohort.TransferSenderId,
            ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy, // not used
            Uln = apprenticeship.Uln,
            DeliveryModel = apprenticeship.DeliveryModel ?? DeliveryModel.Regular,
            TrainingType = apprenticeship.ProgrammeType.Value,
            TrainingCode = apprenticeship.CourseCode,
            StandardUId = apprenticeship.StandardUId,
            TrainingCourseOption = apprenticeship.TrainingCourseOption,
            TrainingCourseVersion = apprenticeship.TrainingCourseVersion,
            StartDate = apprenticeship.StartDate.GetValueOrDefault(),
            EndDate = apprenticeship.EndDate.Value,
            PriceEpisodes = apprenticeship.PriceHistory
                .Select(p => new PriceEpisode
                {
                    FromDate = p.FromDate,
                    ToDate = p.ToDate,
                    Cost = p.Cost,
                    EndPointAssessmentPrice = p.AssessmentPrice,
                    TrainingPrice = p.TrainingPrice
                })
                .ToArray(),
            ContinuationOfId = apprenticeship.ContinuationOfId,
            DateOfBirth = apprenticeship.DateOfBirth.Value,
            ActualStartDate = apprenticeship.ActualStartDate,
            FirstName = apprenticeship.FirstName,
            LastName = apprenticeship.LastName,
            ApprenticeshipHashedId = "", // not used
            LearnerDataId = apprenticeship.LearnerDataId,
            LearningType = LearningType.Apprenticeship
        };
    }
}
