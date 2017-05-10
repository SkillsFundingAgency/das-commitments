using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLocks
{
    public sealed class GetDataLocksValidator : AbstractValidator<GetDataLocksRequest>
    {
        public GetDataLocksValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
        }
    }
}
