using FluentValidation;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships
{
    public class GetEmailOverlappingApprenticeshipsValidator : AbstractValidator<GetEmailOverlappingApprenticeshipsRequest>
    {
        public GetEmailOverlappingApprenticeshipsValidator()
        {
            RuleFor(x => x.OverlappingEmailApprenticeshipRequests).Must(x => x != null);

            RuleForEach(x => x.OverlappingEmailApprenticeshipRequests)
                .SetValidator(new OverlappingEmailApprenticeshipRequestValidator());
        }
    }

    public class OverlappingEmailApprenticeshipRequestValidator : AbstractValidator<ApprenticeshipEmailOverlapValidationRequest>
    {
        public OverlappingEmailApprenticeshipRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }
}