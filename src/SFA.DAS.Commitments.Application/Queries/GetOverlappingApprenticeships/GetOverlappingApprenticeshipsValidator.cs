using System;
using System.Linq;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public class GetOverlappingApprenticeshipsValidator : AbstractValidator<GetOverlappingApprenticeshipsRequest>
    {
        public GetOverlappingApprenticeshipsValidator()
        {
            RuleFor(x => x.OverlappingApprenticeshipRequests).Must(x => x != null && x.Any());

            RuleForEach(x => x.OverlappingApprenticeshipRequests)
                .SetValidator(new OverlappingApprenticeshipRequestValidator());
        }
    }

    public class OverlappingApprenticeshipRequestValidator : AbstractValidator<OverlappingApprenticeshipRequest>
    {
        public OverlappingApprenticeshipRequestValidator()
        {
            RuleFor(x => x.Uln).NotEmpty();
            RuleFor(x => x.DateFrom).NotEmpty();
            RuleFor(x => x.DateTo).NotEmpty();
        }
    }
}
