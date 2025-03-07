using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;

public class GetCohortSupportStatusQueryHandler(Lazy<ProviderCommitmentsDbContext> db, ICohortSupportStatusCalculator calculator)
    : IRequestHandler<GetCohortSupportStatusQuery, GetCohortSupportStatusQueryResult>
{
    public async Task<GetCohortSupportStatusQueryResult> Handle(GetCohortSupportStatusQuery request, CancellationToken cancellationToken)
    {
        var cohort = await db.Value.Cohorts
            .Include(x => x.Apprenticeships)
            .Select(x => new
            {
                x.Id, x.EditStatus, x.ProviderId, NoOfApprovedApprentices = x.Apprenticeships.Count(a=>a.IsApproved == true), x.WithParty, x.LastAction, x.TransferSenderId, x.TransferApprovalStatus
            })
            .FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken);

        if (cohort == null)
            return null;

        var result = new GetCohortSupportStatusQueryResult
        {
            CohortId = cohort.Id,
            NoOfApprentices = cohort.NoOfApprovedApprentices,
            CohortStatus = calculator.GetStatus(
                cohort.EditStatus,
                cohort.NoOfApprovedApprentices > 0, 
                cohort.LastAction,
                cohort.WithParty, 
                cohort.TransferSenderId, 
                cohort.TransferApprovalStatus).GetEnumDescription()
        };            

        return result;
    }
}