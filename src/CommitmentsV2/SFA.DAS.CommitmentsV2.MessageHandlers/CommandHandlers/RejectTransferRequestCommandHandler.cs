using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class RejectTransferRequestCommandHandler : IHandleMessages<RejectTransferRequestCommand>
    {
        private readonly ILogger<RejectTransferRequestCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;

        public RejectTransferRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<RejectTransferRequestCommandHandler> logger)
        {
            _dbContext = dbContext;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(RejectTransferRequestCommand message, IMessageHandlerContext context)
        {
            try
            {
                var transferRequest = await _dbContext.Value.TransferRequests
                    .Include(c => c.Cohort)
                    .Where(x => x.Id == message.TransferRequestId)
                    .SingleAsync();

                transferRequest.Reject(message.UserInfo, message.RejectedOn);

                // Publish legacy event so Tasks can decrement it's counter
                await _legacyTopicMessagePublisher.PublishAsync(new CohortRejectedByTransferSender(
                    message.TransferRequestId,
                    transferRequest.Cohort.EmployerAccountId,
                    transferRequest.Cohort.Id,
                    transferRequest.Cohort.TransferSenderId.Value,
                    message.UserInfo.UserDisplayName,
                    message.UserInfo.UserEmail));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(RejectTransferRequestCommand)}", e);
                throw;
            }
        }
    }
}
