using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortFullyApprovedEventHandler(IMediator mediator) : IHandleMessages<CohortFullyApprovedEvent>
{
    public Task Handle(CohortFullyApprovedEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new ProcessFullyApprovedCohortCommand(message.CohortId, message.AccountId, message.ChangeOfPartyRequestId, message.UserInfo, message.LastApprovedBy));
    }
}