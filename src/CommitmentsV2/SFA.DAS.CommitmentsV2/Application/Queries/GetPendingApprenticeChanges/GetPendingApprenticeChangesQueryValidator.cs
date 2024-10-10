using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingApprenticeChanges;

public class GetPendingApprenticeChangesQueryValidator : AbstractValidator<GetPendingApprenticeChangesQuery>
{
    public GetPendingApprenticeChangesQueryValidator()
    {
        RuleFor(x => x.EmployerAccountId).GreaterThan(0);
    }
}