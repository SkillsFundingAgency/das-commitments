using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

public class GetProviderQueryValidator : AbstractValidator<GetProviderQuery>
{
    public GetProviderQueryValidator()
    {
        RuleFor(q => q.ProviderId).GreaterThan(0).WithMessage("The provider ID must be supplied");
    }
}