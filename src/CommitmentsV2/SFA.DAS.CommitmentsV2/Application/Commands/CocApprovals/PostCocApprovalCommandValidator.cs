using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class PostCocApprovalCommandValidator : AbstractValidator<PostCocApprovalCommand>
{
    public PostCocApprovalCommandValidator()
    {

        RuleFor(model => model.Apprenticeship).NotNull().WithMessage("No Matching Apprenticeship Found");
        RuleFor(model => model.Apprenticeship.Cohort.ProviderId).Equal(x => x.ProviderId).When(model => model.Apprenticeship != null).WithMessage("The UKPRN does not match Provider assigned");
    }
}