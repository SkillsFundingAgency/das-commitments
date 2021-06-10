using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
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
                var transferRequest = await _dbContext.Value.TransferRequests
                    .Include(c => c.Cohort)
                    .Where(x => x.Id == transferRequestId)
                    .SingleAsync(cancellationToken);

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
                var transferRequest = await _dbContext.Value.TransferRequests
                    .Include(c => c.Cohort)
                    .Where(x => x.Id == transferRequestId)
                    .SingleAsync();

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

        public async Task<GetTransferRequestQueryResult> GetTransferRequest(long transferRequestId, long employerAccountId)
        {
            _logger.LogInformation($"Getting Transfer Request {transferRequestId} for employer account {employerAccountId}");

            var query = from t in _dbContext.Value.TransferRequests
                        join c in _dbContext.Value.Cohorts
                            on t.CommitmentId equals c.Id
                        join a in _dbContext.Value.Accounts
                            on c.TransferSenderId equals a.Id
                        join ale in _dbContext.Value.AccountLegalEntities
                            on c.AccountLegalEntityId equals ale.Id
                        where
                            t.Id == transferRequestId
                        select new GetTransferRequestQueryResult
                        {
                            TransferRequestId = t.Id,
                            ReceivingEmployerAccountId = c.EmployerAccountId,
                            CommitmentId = t.CommitmentId,
                            SendingEmployerAccountId = c.TransferSenderId.Value,
                            TransferSenderName = a.Name,
                            LegalEntityName = ale.Name,
                            TransferCost = t.Cost,
                            FundingCap = (int)t.FundingCap,
                            TrainingCourses = t.TrainingCourses,
                            Status = t.Status,
                            ApprovedOrRejectedByUserName = t.TransferApprovalActionedByEmployerName,
                            ApprovedOrRejectedByUserEmail = t.TransferApprovalActionedByEmployerEmail,
                            ApprovedOrRejectedOn = t.TransferApprovalActionedOn
                        };

            var result = await query.FirstOrDefaultAsync();

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
    }
}
