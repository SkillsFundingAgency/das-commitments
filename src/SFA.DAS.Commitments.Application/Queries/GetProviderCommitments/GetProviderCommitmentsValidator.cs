using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderCommitments
{
    public sealed class GetProviderCommitmentsValidator : AbstractValidator<GetProviderCommitmentsRequest>
    {
        public GetProviderCommitmentsValidator()
        {
            RuleFor(request => request.ProviderId).GreaterThan(0);
        }
    }
}
