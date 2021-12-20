using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestCreatedEventHandler : IHandleMessages<TransferRequestCreatedEvent>
    {
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<TransferRequestCreatedEvent> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public TransferRequestCreatedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<TransferRequestCreatedEvent> logger
            , Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(TransferRequestCreatedEvent message, IMessageHandlerContext context)
        {
            try
            {
                var db = _dbContext.Value;
                var transferRequest = await db.TransferRequests.Include(c => c.Cohort)
                    .SingleAsync(x => x.Id == message.TransferRequestId);

                if (transferRequest.AutoApproval)
                {
                    _logger.LogInformation($"AutoApproval set to true - not publishing CohortApprovalByTransferSenderRequested");

                    return;
                }

                _logger.LogInformation($"AutoApproval set to false - publishing CohortApprovalByTransferSenderRequested");

                await _legacyTopicMessagePublisher.PublishAsync(new CohortApprovalByTransferSenderRequested
                {
                    TransferRequestId = message.TransferRequestId,
                    ReceivingEmployerAccountId = transferRequest.Cohort.EmployerAccountId,
                    SendingEmployerAccountId = transferRequest.Cohort.TransferSenderId.Value,
                    TransferCost = transferRequest.Cost,
                    CommitmentId = transferRequest.CommitmentId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish CohortApprovalByTransferSenderRequested");
                throw;
            }
        }
    }
}