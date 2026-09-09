using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommandValidator : AbstractValidator<CocApprovalCommand>
{
    public CocApprovalCommandValidator()
    {
        RuleFor(x => x.CocApprovalDetails)
            .NotNull()
            .WithMessage("CocApprovalDetails is required.")
            .SetValidator(new CocApprovalValidator());
    }
}