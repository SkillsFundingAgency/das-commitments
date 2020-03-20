using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class RejectTransferRequestCommandHandler : IHandleMessages<RejectTransferRequestCommand>
    {
        private readonly ILogger<RejectTransferRequestCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public RejectTransferRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<RejectTransferRequestCommandHandler> logger)
        {
            _dbContext = dbContext;
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

                if (transferRequest.Status == TransferApprovalStatus.Rejected)
                {
                    _logger.LogWarning($"Cohort {message.TransferRequestId} has already Rejected");
                    return;
                }

                transferRequest.Reject(message.UserInfo, message.RejectedOn);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(RejectTransferRequestCommand)}", e);
                throw;
            }
        }
    }
}
