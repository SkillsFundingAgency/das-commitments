using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments
{
    public sealed class GetEmployerCommitmentsValidator : AbstractValidator<GetEmployerCommitmentsRequest>
    {
        public GetEmployerCommitmentsValidator()
        {
            RuleFor(request => request.AccountId).GreaterThan(0);
        }
    }
}
