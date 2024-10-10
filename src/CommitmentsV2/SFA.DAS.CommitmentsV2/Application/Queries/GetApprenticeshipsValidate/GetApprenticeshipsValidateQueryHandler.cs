using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;

public class GetApprenticeshipsValidateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprenticeshipsValidateQuery, GetApprenticeshipsValidateQueryResult>
{
    public async Task<GetApprenticeshipsValidateQueryResult> Handle(GetApprenticeshipsValidateQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Value
            .Apprenticeships
            .Include(apprenticeship => apprenticeship.Cohort)
            .ThenInclude(cohort => cohort.AccountLegalEntity)
            .Where(apprenticeship => apprenticeship.FirstName == request.FirstName && apprenticeship.LastName == request.LastName && apprenticeship.DateOfBirth == request.DateOfBirth)
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetApprenticeshipsValidateQueryResult
        {
            Apprenticeships = result.Select(a => (ApprenticeshipValidateModel)a)
        };
    }
}