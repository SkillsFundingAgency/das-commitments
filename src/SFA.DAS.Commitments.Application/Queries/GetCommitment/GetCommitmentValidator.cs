using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentValidator : AbstractValidator<GetCommitmentRequest>
    {
        public GetCommitmentValidator()
        {
            RuleFor(request => request.CommitmentId).GreaterThan(0);
        }
    }
}
