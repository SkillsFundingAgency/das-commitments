using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandler : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<TransferRequestApprovedEventHandler> _logger;

        public TransferRequestApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestApprovedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"TransferRequestApprovedEvent received for CohortId : {message.CohortId}, TransferRequestId : {message.TransferRequestId}");

                var db = _dbContext.Value;

                var cohort = await db.Cohorts.Include(c => c.Apprenticeships).SingleAsync(c => c.Id == message.CohortId);
                cohort.Approve(Party.TransferSender, null, message.UserInfo, message.ApprovedOn);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to approve Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
                throw;
            }
        }
    }
}