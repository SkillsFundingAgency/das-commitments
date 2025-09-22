using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.Learning.Types;
using SFA.DAS.Learning.Types.Enums;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipWithdrawnEventHandler : IHandleMessages<ApprenticeshipWithdrawnEvent>
    {
        private readonly ILogger<ApprenticeshipPriceChangedEventHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IMediator _mediator;

        public ApprenticeshipWithdrawnEventHandler(
            ILogger<ApprenticeshipPriceChangedEventHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext, IMediator mediator)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task Handle(ApprenticeshipWithdrawnEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipWithdrawnEvent for apprenticeshipId : {apprenticeshipId}", message.ApprovalsApprenticeshipId);

            var reason = GetWithdrawReason(message);

            switch (reason)
            {
                case WithdrawReason.WithdrawFromStart:
                case WithdrawReason.WithdrawDuringLearning:
                    //NB there may be more logic needed here to complete these 2 reason's scenarios
                    await _mediator.Send(new StopApprenticeshipCommand(
                        message.EmployerAccountId,
                        message.ApprovalsApprenticeshipId,
                        message.LastDayOfLearning,
                        false,
                        Types.UserInfo.System,
                        Types.Party.Employer));
                    break;
            }

            _logger.LogInformation("Successfully completed handling of {eventName}", nameof(ApprenticeshipWithdrawnEvent));
        }

        private static WithdrawReason GetWithdrawReason(ApprenticeshipWithdrawnEvent message)
        {
            if (Enum.TryParse<WithdrawReason>(message.Reason, out WithdrawReason reason))
            {
                return reason;
            }

            return WithdrawReason.Other;
        }
    }
}

