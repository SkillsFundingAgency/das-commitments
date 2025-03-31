using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class LevyAddedToAccountEventHandler(IMediator mediator, ILogger<LevyAddedToAccountEventHandler> logger)
    : IHandleMessages<LevyAddedToAccountEvent>
{
    public async Task Handle(LevyAddedToAccountEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("LevyAddedToAccountEvent event received for Account {AccountId}", message.AccountId);

        await mediator.Send(new UpdateLevyStatusToLevyCommand { AccountId = message.AccountId });
    }
}