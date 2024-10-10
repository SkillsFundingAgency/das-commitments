using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortApprovedByEmployerEventHandler(IMediator mediator, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortApprovedByEmployerEventHandler> logger)
    : IHandleMessages<CohortApprovedByEmployerEvent>
{
    public async Task Handle(CohortApprovedByEmployerEvent message, IMessageHandlerContext context)
    {
        try
        {
            var cohort = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            await legacyTopicMessagePublisher.PublishAsync(new CohortApprovedByEmployer
            {
                AccountId = cohort.AccountId,
                ProviderId = cohort.ProviderId.Value,
                CommitmentId = message.CohortId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish CohortApprovedByEmployer");
            throw;
        }
    }
}