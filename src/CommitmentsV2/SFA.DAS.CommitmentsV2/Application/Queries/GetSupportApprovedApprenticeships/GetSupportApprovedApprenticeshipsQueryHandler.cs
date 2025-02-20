using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;

public class GetSupportApprovedApprenticeshipsQueryHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IMapper<Apprenticeship, SupportApprenticeshipDetails> mapper)
    : IRequestHandler<GetSupportApprovedApprenticeshipsQuery, GetSupportApprovedApprenticeshipsQueryResult>
{
    public async Task<GetSupportApprovedApprenticeshipsQueryResult> Handle(GetSupportApprovedApprenticeshipsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Value
            .Apprenticeships
            .Include(x => x.Cohort)
            .Include(x => x.Cohort.AccountLegalEntity)
            .Include(x => x.Cohort.Provider)
            .Include(x => x.PriceHistory)
            .Include(x => x.FlexibleEmployment)
            .AsQueryable();

        if (request.ApprenticeshipId.HasValue)
        {
            query = query.Where(x => x.Id == request.ApprenticeshipId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Uln))
        {
            query = query.Where(x => x.Uln == request.Uln);
        }

        if (request.CohortId.HasValue)
        {
            query = query.Where(x => x.CommitmentId == request.CohortId.Value);
        }

        var apprenticeships = await query.ToListAsync(cancellationToken);

        var mappedApprenticeshipsTask = apprenticeships.Select(mapper.Map).ToList();
        var mappedApprenticeships = await Task.WhenAll(mappedApprenticeshipsTask);
        
        return new GetSupportApprovedApprenticeshipsQueryResult
        {
            ApprovedApprenticeships = mappedApprenticeships,
        };
    }
}