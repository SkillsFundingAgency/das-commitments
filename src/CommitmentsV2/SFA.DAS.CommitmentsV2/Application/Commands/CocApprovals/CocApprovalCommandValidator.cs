using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommandValidator : AbstractValidator<CocApprovalCommand>
{
    public CocApprovalCommandValidator()
    {
        RuleFor(x => x.CocApprovalDetails)
            .NotNull()
            .When(x => x.Action != AggregrationAction.CancelPrevious)
            .WithMessage("CocApprovalDetails is required.")
            .SetValidator(new CocApprovalValidator());
    }
}