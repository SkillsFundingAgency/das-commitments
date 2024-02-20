using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate
{
    public class GetApprenticeshipsValidateQueryHandler : IRequestHandler<GetApprenticeshipsValidateQuery, GetApprenticeshipsValidateQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetApprenticeshipsValidateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprenticeshipsValidateQueryResult> Handle(GetApprenticeshipsValidateQuery request, CancellationToken cancellationToken)
        {
            var result = await _dbContext.Value
                .Apprenticeships
                .Include(a => a.Cohort)
                .ThenInclude(c => c.AccountLegalEntity)
                .Where(a => a.FirstName == request.FirstName && a.LastName == request.LastName && a.DateOfBirth == request.DateOfBirth)
                .ToListAsync();

            return new GetApprenticeshipsValidateQueryResult()
            {
                Apprenticeships = result.Select(a => (ApprenticeshipValidateModel)a)
            };
        }
    }
}
