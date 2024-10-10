using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain;

public class GetChangeOfEmployerChainQueryValidator : AbstractValidator<GetChangeOfEmployerChainQuery>
{
    public GetChangeOfEmployerChainQueryValidator()
    {
        RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
    }
}