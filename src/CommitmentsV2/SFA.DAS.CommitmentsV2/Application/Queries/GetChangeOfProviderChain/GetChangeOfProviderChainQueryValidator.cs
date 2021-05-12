using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain
{
    public class GetChangeOfProviderChainQueryValidator : AbstractValidator<GetChangeOfProviderChainQuery>
    {
        public GetChangeOfProviderChainQueryValidator()
        {
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
        }
    }
}
