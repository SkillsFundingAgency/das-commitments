using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class CocApprovalValidator : AbstractValidator<CocApprovalDetails>
{
    public CocApprovalValidator()
    {
        RuleFor(x => x.Apprenticeship)
            .NotNull()
            .WithMessage("No Matching Apprenticeship Found");

        RuleFor(x => x.Apprenticeship.Cohort.ProviderId)
            .Equal(x => x.ProviderId)
            .When(x => x.Apprenticeship != null)
            .WithMessage("The UKPRN does not match Provider assigned");
    }
}
