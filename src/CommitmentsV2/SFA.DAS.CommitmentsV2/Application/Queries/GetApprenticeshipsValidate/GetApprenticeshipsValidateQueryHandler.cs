using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;

public class GetApprenticeshipsValidateQueryHandler : IRequestHandler<GetApprenticeshipsValidateQuery, GetApprenticeshipsValidateQueryResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    public GetApprenticeshipsValidateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) => _dbContext = dbContext;

    public async Task<GetApprenticeshipsValidateQueryResult> Handle(GetApprenticeshipsValidateQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Value
            .Apprenticeships
            .Include(apprenticeship => apprenticeship.Cohort)
            .ThenInclude(cohort => cohort.AccountLegalEntity)
            .Where(apprenticeship => apprenticeship.FirstName == request.FirstName && apprenticeship.LastName == request.LastName && apprenticeship.DateOfBirth == request.DateOfBirth)
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetApprenticeshipsValidateQueryResult()
        {
            Apprenticeships = result.Select(a => (ApprenticeshipValidateModel)a)
        };
    }
}