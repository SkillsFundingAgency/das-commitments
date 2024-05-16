using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class LevyAddedToAccountEventHandler : IHandleMessages<LevyAddedToAccount>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LevyAddedToAccountEventHandler> _logger;

        public LevyAddedToAccountEventHandler(IMediator mediator, ILogger<LevyAddedToAccountEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(LevyAddedToAccount message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"LevyAddedToAccount event received for Account {message.AccountId}");
            await _mediator.Send(new UpdateLevyStatusToLevyCommand { AccountId = message.AccountId });
        }
    }
}
