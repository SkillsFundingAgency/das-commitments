using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class BulkUploadIntoCohortCompletedEventHandler : IHandleMessages<BulkUploadIntoCohortCompletedEvent>
    {
        private readonly IMediator _mediator;

        public BulkUploadIntoCohortCompletedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(BulkUploadIntoCohortCompletedEvent message, IMessageHandlerContext context)
        {
            var response = await _mediator.Send(new GetDraftApprenticeshipCreatedEventsForCohortQuery(message.ProviderId, message.CohortId,
                message.NumberOfApprentices, message.UploadedOn));

            await Task.WhenAll(response.DraftApprenticeshipCreatedEvents.Select(context.Publish)).ConfigureAwait(false);
        }
    }
}