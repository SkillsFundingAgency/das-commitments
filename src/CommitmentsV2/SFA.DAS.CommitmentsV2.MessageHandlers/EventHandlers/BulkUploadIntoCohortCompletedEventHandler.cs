using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class BulkUploadIntoCohortCompletedEventHandler(IMediator mediator) : IHandleMessages<BulkUploadIntoCohortCompletedEvent>
{
    public async Task Handle(BulkUploadIntoCohortCompletedEvent message, IMessageHandlerContext context)
    {
        var query = new GetDraftApprenticeshipCreatedEventsForCohortQuery(
            message.ProviderId,
            message.CohortId,
            message.NumberOfApprentices,
            message.UploadedOn
        );

        var response = await mediator.Send(query);

        await Task.WhenAll(response.DraftApprenticeshipCreatedEvents.Select(context.Publish)).ConfigureAwait(false);
    }
}