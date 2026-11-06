using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;

public class GetApprenticeshipLockedFieldsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, CocApprovalsConfiguration cocApprovals) : IRequestHandler<GetApprenticeshipLockedFieldsQuery, GetApprenticeshipLockedFieldsQueryResult>
{
    public async Task<GetApprenticeshipLockedFieldsQueryResult> Handle(GetApprenticeshipLockedFieldsQuery request, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships.Include(x => x.Cohort).FirstOrDefaultAsync(x => x.Id == request.ApprenticeshipId, cancellationToken);
        if (apprenticeship == null)
        {
            return null;
        }

        var providerId = apprenticeship.Cohort.ProviderId;
        var retVal = new GetApprenticeshipLockedFieldsQueryResult
        {
            LockedManualFields = new List<string>()
        };

        if (cocApprovals.UkPrns.Contains(providerId))
        {
            retVal.LockedManualFields = cocApprovals.LockedManualFields;
        }
        
        return retVal;
    }
}