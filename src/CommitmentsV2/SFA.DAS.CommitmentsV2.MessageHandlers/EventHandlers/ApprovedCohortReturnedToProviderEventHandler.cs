using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprovedCohortReturnedToProviderEventHandler(IMediator mediator, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprovedCohortReturnedToProviderEventHandler> logger)
    : IHandleMessages<ApprovedCohortReturnedToProviderEvent>
{
    public async Task Handle(ApprovedCohortReturnedToProviderEvent message, IMessageHandlerContext context)
    {
        try
        {
            var cohort = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            if (cohort == null)
            {
                logger.LogInformation("Cohort {CohortId} not found when processing ApprovedCohortReturnedToProviderEvent", message.CohortId);
                return;
            }

            await legacyTopicMessagePublisher.PublishAsync(new ApprovedCohortReturnedToProvider
            {
                AccountId = cohort.AccountId,
                ProviderId = cohort.ProviderId.Value,
                CommitmentId = message.CohortId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish ApprovedCohortReturnedToProvider");
            throw;
        }
    }
}