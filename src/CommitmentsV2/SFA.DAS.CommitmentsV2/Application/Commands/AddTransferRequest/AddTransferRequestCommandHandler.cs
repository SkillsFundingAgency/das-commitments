using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

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

            var autoApproval = false;
            if (cohort.PledgeApplicationId.HasValue)
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

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Adding Transfer Request");
            throw;
        }
    }
}