using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

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
                    ApprovedOrRejectedOn = t.TransferApprovalActionedOn,
                    AutoApproval = t.AutoApproval,
                    PledgeApplicationId = t.Cohort.PledgeApplicationId
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

        public async Task<List<EmployerTransferRequestPendingNotification>> GetEmployerTransferRequestPendingNotifications()
        {
            _logger.LogInformation("Getting pending transfer requests for employer accounts");

            var query = _dbContext.Value.TransferRequests
                            .Include(t => t.Cohort)
                            .ThenInclude(c => c.AccountLegalEntity)
                            .Where(tr => tr.Status == TransferApprovalStatus.Pending && tr.AutoApproval == false)
                            .Select(tr => new EmployerTransferRequestPendingNotification
                            {
                                TransferRequestId = tr.Id,
                                ReceivingEmployerAccountId = tr.Cohort.EmployerAccountId,
                                ReceivingLegalEntityName = tr.Cohort.AccountLegalEntity.Name,
                                CohortReference = tr.Cohort.Reference,
                                CommitmentId = tr.CommitmentId,
                                SendingEmployerAccountId = tr.Cohort.TransferSenderId,
                                TransferCost = tr.Cost,
                                Status = tr.Status,
                                ApprovedOrRejectedByUserName = tr.TransferApprovalActionedByEmployerName,
                                ApprovedOrRejectedByUserEmail = tr.TransferApprovalActionedByEmployerEmail,
                                ApprovedOrRejectedOn = tr.TransferApprovalActionedOn,
                                CreatedOn = tr.CreatedOn
                            });

            var results = await query.ToListAsync();

            if (results.Any())
            {
                _logger.LogInformation("Retrieved pending transfer requests for employer accounts");
            }
            else
            {
                _logger.LogInformation($"Cannot find any pending transfer requests for employer accounts");
            }

            return results;
        }

        private async Task<TransferRequest> GetTransferRequest(long id, CancellationToken cancellationToken)
        {
            return await _dbContext.Value.TransferRequests
                    .Include(c => c.Cohort)
                    .ThenInclude(c => c.Apprenticeships)
                    .SingleAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestSummary(long accountId, TransferType? originator, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Transfer Request Summary for employer account {accountId}");

            IEnumerable<TransferRequestsSummaryQueryResult> result = Enumerable.Empty<TransferRequestsSummaryQueryResult>();

            if (originator == null)
            {
                var receiverRequests = await GetTransferRequestsForReceiver(accountId);
                var senderTransfers = await GetTransferRequestsForSender(accountId);
                result = Concatenate(receiverRequests.TransferRequestsSummaryQueryResult, senderTransfers.TransferRequestsSummaryQueryResult);
            }
            else if (originator == TransferType.AsSender)
            {
                result = (await GetTransferRequestsForSender(accountId)).TransferRequestsSummaryQueryResult;
            }
            else if (originator == TransferType.AsReceiver)
            {
                result = (await GetTransferRequestsForReceiver(accountId)).TransferRequestsSummaryQueryResult;
            }

            if (result.Any())
            {
                _logger.LogInformation($"Retrieved Transfer Request Summary for employer account {accountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Transfer Request Summary for employer account {accountId}");
            }

            return new GetTransferRequestsSummaryQueryResult
            {
                TransferRequestsSummaryQueryResult = result
            };
        }

        private async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestsForReceiver(long accountId)
        {
            var result = await _dbContext.Value.TransferRequests
                            .Include(t => t.Cohort)
                            .ThenInclude(c => c.AccountLegalEntity)
                            .Where(w => w.Cohort.EmployerAccountId == accountId)
                            .Select(t => new TransferRequestsSummaryQueryResult
                            {
                                ApprovedOrRejectedByUserName = t.TransferApprovalActionedByEmployerName,
                                ApprovedOrRejectedByUserEmail = t.TransferApprovalActionedByEmployerEmail,
                                ApprovedOrRejectedOn = t.TransferApprovalActionedOn,
                                CohortReference = t.Cohort.Reference,
                                CommitmentId = t.CommitmentId,
                                CreatedOn = t.CreatedOn,
                                FundingCap = (int)t.FundingCap,
                                ReceivingEmployerAccountId = t.Cohort.EmployerAccountId,
                                SendingEmployerAccountId = t.Cohort.TransferSenderId.Value,
                                Status = t.Status,
                                TransferCost = t.Cost,
                                TransferRequestId = t.Id,
                                TransferType = TransferType.AsReceiver

                            }).ToListAsync();

            if (result.Any())
            {
                _logger.LogInformation($"Retrieved Transfer Requests for Receiver for employer account {accountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot Retrieved Transfer Requests for Receiver for employer account {accountId}");
            }

            return new GetTransferRequestsSummaryQueryResult
            {
                TransferRequestsSummaryQueryResult = result.OrderBy(o => o.CommitmentId).ThenBy(t => t.CreatedOn)
            };
        }

        private async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestsForSender(long accountId)
        {
            var result = await _dbContext.Value.TransferRequests
                            .Include(t => t.Cohort)
                            .ThenInclude(c => c.AccountLegalEntity)
                            .Where(w => w.Cohort.TransferSenderId.Value == accountId)
                            .Select(t => new TransferRequestsSummaryQueryResult
                            {
                                ApprovedOrRejectedByUserName = t.TransferApprovalActionedByEmployerName,
                                ApprovedOrRejectedByUserEmail = t.TransferApprovalActionedByEmployerEmail,
                                ApprovedOrRejectedOn = t.TransferApprovalActionedOn,
                                CohortReference = t.Cohort.Reference,
                                CommitmentId = t.CommitmentId,
                                CreatedOn = t.CreatedOn,
                                FundingCap = (int)t.FundingCap,
                                ReceivingEmployerAccountId = t.Cohort.EmployerAccountId,
                                SendingEmployerAccountId = t.Cohort.TransferSenderId.Value,
                                Status = t.Status,
                                TransferCost = t.Cost,
                                TransferRequestId = t.Id,
                                TransferType = TransferType.AsSender

                            }).ToListAsync();

            if (result.Any())
            {
                _logger.LogInformation($"Retrieved Transfer Request  summary for employer account {accountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Transfer Request summary for employer account {accountId}");
            }

            return new GetTransferRequestsSummaryQueryResult
            {
                TransferRequestsSummaryQueryResult = result.OrderBy(o => o.CommitmentId).ThenBy(t => t.CreatedOn)
            };
        }

        public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }
    }
}
