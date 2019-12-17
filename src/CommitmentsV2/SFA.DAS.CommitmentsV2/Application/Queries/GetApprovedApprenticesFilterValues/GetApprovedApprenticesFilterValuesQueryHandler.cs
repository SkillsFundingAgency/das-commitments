using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryHandler : IRequestHandler<GetApprovedApprenticesFilterValuesQuery, GetApprovedApprenticesFilterValuesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprovedApprenticesFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprovedApprenticesFilterValuesResponse> Handle(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            var employerNames = await _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);

            return new GetApprovedApprenticesFilterValuesResponse
            {
                EmployerNames = employerNames
            };
        }
    }
}
