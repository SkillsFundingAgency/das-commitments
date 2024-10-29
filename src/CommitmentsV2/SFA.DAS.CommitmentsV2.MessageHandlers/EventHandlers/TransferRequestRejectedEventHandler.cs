using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestRejectedEventHandler : IHandleMessages<TransferRequestRejectedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<TransferRequestRejectedEvent> _logger;

        public TransferRequestRejectedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestRejectedEvent> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(TransferRequestRejectedEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"TransferRequestRejectedEvent received for CohortId : {message.CohortId}, TransferRequestId : {message.TransferRequestId}");

                var db = _dbContext.Value;

                var cohort = await _dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId);
                cohort.RejectTransferRequest(message.UserInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to reject Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
                throw;
            }
        }
    }
}