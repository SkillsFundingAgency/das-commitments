using FluentValidation;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public class GetOverlappingApprenticeshipsValidator : AbstractValidator<GetOverlappingApprenticeshipsRequest>
    {
        public GetOverlappingApprenticeshipsValidator()
        {
            RuleFor(x => x.OverlappingApprenticeshipRequests).Must(x => x != null);

            RuleForEach(x => x.OverlappingApprenticeshipRequests)
                .SetValidator(new OverlappingApprenticeshipRequestValidator());
        }
    }

    public class OverlappingApprenticeshipRequestValidator : AbstractValidator<ApprenticeshipOverlapValidationRequest>
    {
        public OverlappingApprenticeshipRequestValidator()
        {
            RuleFor(x => x.Uln).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }
}