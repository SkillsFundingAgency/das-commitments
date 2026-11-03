using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Exceptions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdateStatus;

public class GetApprenticeshipUpdateStatusQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, CocApprovalsConfiguration cocApprovals) : IRequestHandler<GetApprenticeshipUpdateStatusQuery, GetApprenticeshipUpdateStatusQueryResult>
{
    public async Task<GetApprenticeshipUpdateStatusQueryResult> Handle(GetApprenticeshipUpdateStatusQuery request, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships.Include(x => x.Cohort).FirstOrDefaultAsync(x => x.Id == request.ApprenticeshipId, cancellationToken);
        var retVal = new GetApprenticeshipUpdateStatusQueryResult
        {
            LockedManualFields = new List<string>()
        };

        if (apprenticeship == null)
        {
            return retVal;
        }

        var providerId = apprenticeship.Cohort.ProviderId;  

        if(cocApprovals?.UkPrns != null && cocApprovals.UkPrns.Contains(providerId))
        {
            retVal.LockedManualFields = cocApprovals.LockedManualFields;
        }
        
        return retVal;
    }
}