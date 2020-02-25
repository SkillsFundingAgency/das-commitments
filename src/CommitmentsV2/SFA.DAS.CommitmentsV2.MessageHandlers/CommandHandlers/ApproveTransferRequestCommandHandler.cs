using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ApproveTransferRequestCommandHandler : IHandleMessages<ApproveTransferRequestCommand>
    {
        private readonly ILogger<ApproveTransferRequestCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ApproveTransferRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApproveTransferRequestCommandHandler> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(ApproveTransferRequestCommand message, IMessageHandlerContext context)
        {
            try
            {
                var transferRequest = await _dbContext.Value.TransferRequests.Include(c => c.Cohort).Where(x=>x.Id == message.TransferRequestId).SingleAsync();
                if (transferRequest.Status == TransferApprovalStatus.Approved)
                {
                    _logger.LogWarning($"Transfer Request {transferRequest.Id} has already been approved");
                    return;
                }
                
                transferRequest.Approve(message.UserInfo, message.ApprovedOn);
            }
            catch (Exception e)
            {
                _logger.LogError("Error processing TransferSenderApproveCohortCommand", e);
                throw;
            }
        }
    }
}