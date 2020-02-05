using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

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
                var cohort = await _dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId);
                cohort.TransferRequestRejectedReturnCohortToEmployer();
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