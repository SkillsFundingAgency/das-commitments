using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class LevyAddedToAccountEventHandler(IMediator mediator, ILogger<LevyAddedToAccountEventHandler> logger)
    : IHandleMessages<LevyAddedToAccountEvent>
{
    public async Task Handle(LevyAddedToAccountEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("LevyAddedToAccountEvent event received for Account {AccountId}", message.AccountId);

        await mediator.Send(new UpdateAccountLevyStatusCommand
        {
            AccountId = message.AccountId,
            LevyStatus = ApprenticeshipEmployerType.Levy
        });
    }
}
