using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class TransferRequestDomainService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<TransferRequestDomainService> logger)
    : ITransferRequestDomainService
{
    public async Task ApproveTransferRequest(long transferRequestId, UserInfo userInfo, DateTime approvedOn, CancellationToken cancellationToken)
    {
        try
        {
            var transferRequest = await GetTransferRequest(transferRequestId, cancellationToken);
            if (transferRequest.Status == TransferApprovalStatus.Approved)
            {
                logger.LogWarning("Transfer Request {Id} has already been approved", transferRequest.Id);
                return;
            }

            transferRequest.Approve(userInfo, approvedOn);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing {TypeName}", nameof(ApproveTransferRequest));
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
                logger.LogWarning("Transfer Request {TransferRequestId} has already been rejected", transferRequestId);
                return;
            }

            transferRequest.Reject(userInfo, rejectedOn);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing {TypeName}", nameof(RejectTransferRequest));
            throw;
        }
    }

    public async Task<GetTransferRequestQueryResult> GetTransferRequest(long transferRequestId, long employerAccountId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting Transfer Request {TransferRequestId} for employer account {EmployerAccountId}", transferRequestId, employerAccountId);

        var result = await dbContext.Value.TransferRequests
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
            logger.LogInformation("Retrieved Transfer Request {TransferRequestId} for employer account {EmployerAccountId}", transferRequestId, employerAccountId);
        }
        else
        {
            logger.LogInformation("Cannot find Transfer Request {TransferRequestId} for employer account {EmployerAccountId}", transferRequestId, employerAccountId);
        }

        return result;
    }

    public async Task<List<EmployerTransferRequestPendingNotification>> GetEmployerTransferRequestPendingNotifications()
    {
        logger.LogInformation("Getting pending transfer requests for employer accounts");

        var query = dbContext.Value.TransferRequests
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

        if (results.Count != 0)
        {
            logger.LogInformation("Retrieved pending transfer requests for employer accounts");
        }
        else
        {
            logger.LogInformation($"Cannot find any pending transfer requests for employer accounts");
        }

        return results;
    }

    private async Task<TransferRequest> GetTransferRequest(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Value.TransferRequests
            .Include(c => c.Cohort)
            .ThenInclude(c => c.Apprenticeships)
            .SingleAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestSummary(long accountId, TransferType? originator, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting Transfer Request Summary for employer account {AccountId}", accountId);

        IEnumerable<TransferRequestsSummaryQueryResult> result = [];

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
            logger.LogInformation("Retrieved Transfer Request Summary for employer account {AccountId}", accountId);
        }
        else
        {
            logger.LogInformation("Cannot find Transfer Request Summary for employer account {AccountId}", accountId);
        }

        return new GetTransferRequestsSummaryQueryResult
        {
            TransferRequestsSummaryQueryResult = result
        };
    }

    private async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestsForReceiver(long accountId)
    {
        var result = await dbContext.Value.TransferRequests
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

        if (result.Count != 0)
        {
            logger.LogInformation("Retrieved Transfer Requests for Receiver for employer account {AccountId}", accountId);
        }
        else
        {
            logger.LogInformation("Cannot Retrieved Transfer Requests for Receiver for employer account {AccountId}", accountId);
        }

        return new GetTransferRequestsSummaryQueryResult
        {
            TransferRequestsSummaryQueryResult = result.OrderBy(o => o.CommitmentId).ThenBy(t => t.CreatedOn)
        };
    }

    private async Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestsForSender(long accountId)
    {
        var result = await dbContext.Value.TransferRequests
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

        if (result.Count != 0)
        {
            logger.LogInformation("Retrieved Transfer Request  summary for employer account {AccountId}", accountId);
        }
        else
        {
            logger.LogInformation("Cannot find Transfer Request summary for employer account {AccountId}", accountId);
        }

        return new GetTransferRequestsSummaryQueryResult
        {
            TransferRequestsSummaryQueryResult = result.OrderBy(o => o.CommitmentId).ThenBy(t => t.CreatedOn)
        };
    }

    private static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
    {
        return lists.SelectMany(x => x);
    }
}