using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class TransferRequestDomainService : ITransferRequestDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<TransferRequestDomainService> _logger;

        public TransferRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<TransferRequestDomainService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task ApproveTransferRequest(long transferRequestId, UserInfo userInfo, DateTime approvedOn, CancellationToken cancellationToken)
        {
            try
            {
                var transferRequest = await GetTransferRequest(transferRequestId, cancellationToken);
                if (transferRequest.Status == TransferApprovalStatus.Approved)
                {
                    _logger.LogWarning($"Transfer Request {transferRequest.Id} has already been approved");
                    return;
                }

                transferRequest.Approve(userInfo, approvedOn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(ApproveTransferRequest)}");
                throw;
            }
        }

        public async Task RejectTransferRequest(long transferRequestId, UserInfo userInfo, DateTime rejectedOn, CancellationToken cancellationToken)
        {
            try
            {
                var transferRequest = await GetTransferRequest(transferRequestId, cancellationToken);
                if (transferRequest.Status == TransferApprovalStatus.Rejected)
                {
                    _logger.LogWarning($"Transfer Request {transferRequestId} has already been rejected");
                    return;
                }

                transferRequest.Reject(userInfo, rejectedOn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(RejectTransferRequest)}");
                throw;
            }
        }

        public async Task<GetTransferRequestQueryResult> GetTransferRequest(long transferRequestId, long employerAccountId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Transfer Request {transferRequestId} for employer account {employerAccountId}");

            var result = await _dbContext.Value.TransferRequests
                .Include(t => t.Cohort)
                .ThenInclude(c => c.AccountLegalEntity)
                .ThenInclude(ale => ale.Account)
                .Select(t => new GetTransferRequestQueryResult
                {
                    TransferRequestId = t.Id,
                    ReceivingEmployerAccountId = t.Cohort.EmployerAccountId,
                    CommitmentId = t.CommitmentId,
                    SendingEmployerAccountId = t.Cohort.TransferSenderId.Value,
                    TransferSenderName = t.Cohort.AccountLegalEntity.Account.Name,
                    LegalEntityName = t.Cohort.AccountLegalEntity.Name,
                    TransferCost = t.Cost,
                    FundingCap = (int)t.FundingCap,
                    TrainingCourses = t.TrainingCourses,
                    Status = t.Status,
                    ApprovedOrRejectedByUserName = t.TransferApprovalActionedByEmployerName,
                    ApprovedOrRejectedByUserEmail = t.TransferApprovalActionedByEmployerEmail,
                    ApprovedOrRejectedOn = t.TransferApprovalActionedOn
                })
                .FirstOrDefaultAsync(t => t.TransferRequestId == transferRequestId, cancellationToken);

            if (result != null)
            {
                _logger.LogInformation($"Retrieved Transfer Request {transferRequestId} for employer account {employerAccountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Transfer Request {transferRequestId} for employer account {employerAccountId}");
            }

            return result;
        }

        private async Task<TransferRequest> GetTransferRequest(long id, CancellationToken cancellationToken)
        {
            return await _dbContext.Value.TransferRequests
                    .Include(c => c.Cohort)
                    .SingleAsync(x => x.Id == id, cancellationToken);
        }
    }
}
