using Microsoft.EntityFrameworkCore;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Apprenticeships.Types.Enums;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipWithdrawnEventHandler : IHandleMessages<ApprenticeshipWithdrawnEvent>
    {
        private readonly ILogger<ApprenticeshipPriceChangedEventHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ApprenticeshipWithdrawnEventHandler(
            ILogger<ApprenticeshipPriceChangedEventHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(ApprenticeshipWithdrawnEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipWithdrawnEvent for apprenticeshipId : {apprenticeshipId}", message.ApprenticeshipId);

            var reason = GetWithdrawReason(message);

            switch (reason)
            {
                case WithdrawReason.WithdrawFromStart:
                    // Will be developed in a future story
                    break;

                case WithdrawReason.WithdrawDuringLearning:
                    // Will be developed in a future story
                    break;

                case WithdrawReason.WithdrawFromBeta:
                    await WithdrawFromPaymentSimplificationBeta(message);
                    break;
            }

            _logger.LogInformation("Successfully completed handling of {eventName}", nameof(ApprenticeshipWithdrawnEvent));
        }

        private async Task WithdrawFromPaymentSimplificationBeta(ApprenticeshipWithdrawnEvent message)
        {
            var apprenticeship = _dbContext.Value.Apprenticeships.Single(x => x.Id == message.ApprenticeshipId);
            apprenticeship.IsOnFlexiPaymentPilot = false;
            _dbContext.Value.Update(apprenticeship);
            _dbContext.Value.SaveChanges();
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

