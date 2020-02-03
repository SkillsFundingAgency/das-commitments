using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandler : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<TransferRequestApprovedEvent> _logger;

        public TransferRequestApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestApprovedEvent> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            try
            {
                var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, new CancellationToken());
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