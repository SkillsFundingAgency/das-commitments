using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class PostCocApprovalCommandValidator : AbstractValidator<PostCocApprovalCommand>
{
    public PostCocApprovalCommandValidator()
    {
        RuleFor(x => x.CocApprovalDetails)
                    .NotNull()
                    .WithMessage("CocApprovalCommand is required.")
                    .SetValidator(new CocApprovalValidator());
    }
}