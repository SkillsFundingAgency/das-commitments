using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    public class GetPendingApprenticeshipUpdateValidator: AbstractValidator<GetPendingApprenticeshipUpdateRequest>
    {
        public GetPendingApprenticeshipUpdateValidator()
        {
            RuleFor(x => x.ApprenticeshipId).Must(x => x > 0);
        }
    }
}
