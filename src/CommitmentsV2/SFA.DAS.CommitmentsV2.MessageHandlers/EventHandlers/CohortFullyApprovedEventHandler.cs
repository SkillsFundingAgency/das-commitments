using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortFullyApprovedEventHandler : IHandleMessages<CohortFullyApprovedEvent>
    {
        private readonly IMediator _mediator;

        public CohortFullyApprovedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(CohortFullyApprovedEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new ProcessFullyApprovedCohortCommand(message.CohortId, message.AccountId, message.ChangeOfPartyRequestId, message.UserInfo, message.LastApprovedBy));
        }
    }
}