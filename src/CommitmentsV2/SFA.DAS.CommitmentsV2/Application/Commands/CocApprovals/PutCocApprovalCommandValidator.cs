using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class PutCocApprovalCommandValidator : AbstractValidator<PutCocApprovalCommand>
{
    public PutCocApprovalCommandValidator()
    {
        RuleFor(x => x.CocApprovalDetails)
                    .NotNull()
                    .WithMessage("CocApprovalDetails is required.")
                    .SetValidator(new CocApprovalValidator());
    }
}