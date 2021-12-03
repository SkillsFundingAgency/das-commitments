using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                    .ThenInclude(c => c.Apprenticeships)
                    .SingleAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestSummary(long accountId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Transfer Request Summary for employer account {accountId}");

            var receiverRequests = await GetTransferRequestsForReceiver(accountId);
            var senderTransfers = await GetTransferRequestsForSender(accountId);
            var result = Concatenate(receiverRequests.TransferRequestsSummaryQueryResult, senderTransfers.TransferRequestsSummaryQueryResult);

            if (result != null)
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
            /*var receiverEmployerAccountIdParam = new SqlParameter("@receiverEmployerAccountId", accountId);

            var results = await _dbContext.Value.TransferRequestSummary
                             .FromSql("exec GetTransferRequestsForReceiver @receiverEmployerAccountId", receiverEmployerAccountIdParam)
                             .ToListAsync();*/

            var transferRequestsForReceiverSummary = await (from tran in _dbContext.Value.TransferRequests
                                                            join coh in _dbContext.Value.Cohorts on tran.CommitmentId equals coh.Id
                                               join ale in _dbContext.Value.AccountLegalEntities on coh.AccountLegalEntityId equals ale.Id
                                               where coh.EmployerAccountId == accountId
                                               select new TransferRequestSummary
                                               {
                                                    ApprovedOrRejectedByUserName = tran.TransferApprovalActionedByEmployerName,
                                                    ApprovedOrRejectedByUserEmail = tran.TransferApprovalActionedByEmployerEmail,
                                                    ApprovedOrRejectedOn = tran.TransferApprovalActionedOn,
                                                    CohortReference = coh.Reference,
                                                    CommitmentId = tran.CommitmentId,
                                                    CreatedOn = tran.CreatedOn,
                                                    FundingCap = (int)tran.FundingCap, //TODO : change the model to decimal
                                                    ReceivingEmployerAccountId = coh.EmployerAccountId,
                                                    SendingEmployerAccountId = (long)coh.TransferSenderId,
                                                    Status = tran.Status,
                                                    TransferCost = tran.Cost,
                                                    TransferRequestId = tran.Id
                                                   
                                               }).ToListAsync();


            var finaloutcome = transferRequestsForReceiverSummary.Where(x => x.ReceivingEmployerAccountId == accountId)
                 .OrderBy(o => o.CommitmentId)
                 .ThenBy(t => t.CreatedOn);


            return new GetTransferRequestsSummaryQueryResult
            {
                TransferRequestsSummaryQueryResult = finaloutcome.Select(x => MapFrom(x, TransferType.AsReceiver))
                //TransferRequestsSummaryQueryResult =  results.Select(x => MapFrom(x, TransferType.AsReceiver))                
            };
        }

        private async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestsForSender(long accountId)
        {
           /* var senderEmployerAccountIdParam = new SqlParameter("@senderEmployerAccountId", accountId);

            var results = await _dbContext.Value.TransferRequestSummary
                             .FromSql("exec GetTransferRequestsForSender @senderEmployerAccountId", senderEmployerAccountIdParam)
                             .ToListAsync();*/


            var transferRequestsForReceiverSummary = await (from tran in _dbContext.Value.TransferRequests
                                                            join coh in _dbContext.Value.Cohorts on tran.CommitmentId equals coh.Id
                                                            join ale in _dbContext.Value.AccountLegalEntities on coh.AccountLegalEntityId equals ale.Id
                                                            where coh.EmployerAccountId == accountId
                                                            select new TransferRequestSummary
                                                            {
                                                                ApprovedOrRejectedByUserName = tran.TransferApprovalActionedByEmployerName,
                                                                ApprovedOrRejectedByUserEmail = tran.TransferApprovalActionedByEmployerEmail,
                                                                ApprovedOrRejectedOn = tran.TransferApprovalActionedOn,
                                                                CohortReference = coh.Reference,
                                                                CommitmentId = tran.CommitmentId,
                                                                CreatedOn = tran.CreatedOn,
                                                                FundingCap = (int)tran.FundingCap, //TODO : change the model to decimal
                                                                ReceivingEmployerAccountId = coh.EmployerAccountId,
                                                                SendingEmployerAccountId = (long)coh.TransferSenderId,
                                                                Status = tran.Status,
                                                                TransferCost = tran.Cost,
                                                                TransferRequestId = tran.Id

                                                            }).ToListAsync();


            var finaloutcome = transferRequestsForReceiverSummary.Where(x => x.SendingEmployerAccountId == accountId)
                 .OrderBy(o => o.CommitmentId)
                 .ThenBy(t => t.CreatedOn);


            return new GetTransferRequestsSummaryQueryResult
            {
                TransferRequestsSummaryQueryResult = finaloutcome.Select(x => MapFrom(x, TransferType.AsSender))                              
            };

            //return new GetTransferRequestsSummaryQueryResult
            //{
            //    TransferRequestsSummaryQueryResult = results.Select(x => MapFrom(x, TransferType.AsSender))
            //};
        }
        public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }

        public TransferRequestsSummaryQueryResult MapFrom(TransferRequestSummary source, TransferType transferType)
        {
            if (source == null)
                return null;

            return new TransferRequestsSummaryQueryResult
            {
                TransferRequestId = source.TransferRequestId,
                ReceivingEmployerAccountId = source.ReceivingEmployerAccountId,
                CommitmentId = source.CommitmentId,
                SendingEmployerAccountId = source.SendingEmployerAccountId,
                CohortReference = source.CohortReference,
                TransferCost = source.TransferCost,
                TransferType = transferType,
                Status = (TransferApprovalStatus)source.Status,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn,
                FundingCap = source.FundingCap
            };
        }       
    }
}
