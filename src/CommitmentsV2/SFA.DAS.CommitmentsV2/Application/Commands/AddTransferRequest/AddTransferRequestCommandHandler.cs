using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;

public class AddTransferRequestCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IFundingCapService fundingCapService,
    ILogger<AddTransferRequestCommandHandler> logger,
    IApprovalsOuterApiClient apiClient)
    : IRequestHandler<AddTransferRequestCommand>
{
    public async Task Handle(AddTransferRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var db = dbContext.Value;

            var cohort = await db.GetCohortAggregate(request.CohortId, cancellationToken: cancellationToken);

            var transferSenderIsNonLevy = await IsTransferSenderNonLevy(db, cohort.TransferSenderId, cancellationToken);

            var autoApproval = false;
            if (!transferSenderIsNonLevy && cohort.PledgeApplicationId.HasValue)
            {
                var apiRequest = new GetPledgeApplicationRequest(cohort.PledgeApplicationId.Value);
                var pledgeApplication = await apiClient.Get<PledgeApplication>(apiRequest);
                autoApproval = pledgeApplication.AutomaticApproval;
            }

            var fundingCapSummary = await fundingCapService.FundingCourseSummary(cohort.Apprenticeships);

            cohort.AddTransferRequest(
                JsonConvert.SerializeObject(fundingCapSummary.Select(x => new {x.CourseTitle, x.ApprenticeshipCount})),
                fundingCapSummary.Sum(x => x.CappedCost), 
                fundingCapSummary.Sum(x => x.ActualCap),
                request.LastApprovedByParty,
                autoApproval);

            if (transferSenderIsNonLevy)
            {
                var transferRequest = cohort.TransferRequests.Single(tr => tr.Status == TransferApprovalStatus.Pending);
                transferRequest.Cohort = cohort;
                transferRequest.Reject(UserInfo.System, DateTime.UtcNow);

                logger.LogInformation(
                    "Auto-rejected transfer request for cohort {CohortId} because transfer sender {TransferSenderId} is NonLevy",
                    cohort.Id,
                    cohort.TransferSenderId);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Adding Transfer Request");
            throw;
        }
    }

    private static async Task<bool> IsTransferSenderNonLevy(
        ProviderCommitmentsDbContext db,
        long? transferSenderId,
        CancellationToken cancellationToken)
    {
        if (!transferSenderId.HasValue)
        {
            return false;
        }

        var levyStatus = await db.Accounts
            .Where(a => a.Id == transferSenderId.Value)
            .Select(a => (ApprenticeshipEmployerType?)a.LevyStatus)
            .SingleOrDefaultAsync(cancellationToken);

        return levyStatus == ApprenticeshipEmployerType.NonLevy;
    }
}
