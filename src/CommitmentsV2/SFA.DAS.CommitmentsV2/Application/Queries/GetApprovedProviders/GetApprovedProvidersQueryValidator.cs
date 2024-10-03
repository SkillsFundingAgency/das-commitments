using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;

public class GetApprovedProvidersQueryValidator : AbstractValidator<GetApprovedProvidersQuery>
{
    public GetApprovedProvidersQueryValidator()
    {
        RuleFor(q => q.AccountId).GreaterThan(0).WithMessage("The account ID must be supplied");
    }
}