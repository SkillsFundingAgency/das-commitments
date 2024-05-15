using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestRejectedEventHandler : IHandleMessages<TransferRequestRejectedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<TransferRequestRejectedEvent> _logger;

        public TransferRequestRejectedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<TransferRequestRejectedEvent> logger)
        {
            _dbContext = dbContext;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(TransferRequestRejectedEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"TransferRequestRejectedEvent received for CohortId : {message.CohortId}, TransferRequestId : { message.TransferRequestId}");

                var db = _dbContext.Value;

                var cohort = await _dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId);
                cohort.RejectTransferRequest(message.UserInfo);

                var transferRequest = await db.TransferRequests.SingleAsync(x => x.Id == message.TransferRequestId);

                if (transferRequest.AutoApproval)
                {
                    _logger.LogInformation($"AutoApproval set to true - not publishing CohortRejectedByTransferSender");

                    return;
                }

                // Publish legacy event so Tasks can decrement it's counter
                await _legacyTopicMessagePublisher.PublishAsync(new CohortRejectedByTransferSender(
                    message.TransferRequestId,
                    cohort.EmployerAccountId,
                    cohort.Id,
                    cohort.TransferSenderId.Value,
                    message.UserInfo.UserDisplayName,
                    message.UserInfo.UserEmail));

                _logger.LogInformation($"Cohort {message.CohortId} returned to Employer, after TransferRequest {message.TransferRequestId} was rejected");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to reject Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
                throw;
            }
        }
    }
}