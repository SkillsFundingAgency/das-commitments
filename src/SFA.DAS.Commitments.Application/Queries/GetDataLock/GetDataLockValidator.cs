using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLock
{
    public sealed class GetDataLockValidator : AbstractValidator<GetDataLockRequest>
    {
        public GetDataLockValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.DataLockEventId).NotEmpty();
        }
    }
}
