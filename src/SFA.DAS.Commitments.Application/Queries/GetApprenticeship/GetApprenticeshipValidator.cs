using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipValidator : AbstractValidator<GetApprenticeshipRequest>
    {
        public GetApprenticeshipValidator()
        {
            RuleFor(x => x.AccountId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
        }
    }
}
