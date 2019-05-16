using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship
{
    public class UpdateDraftApprenticeshipValidator :  AbstractValidator<UpdateDraftApprenticeshipCommand>
    {
        public UpdateDraftApprenticeshipValidator()
        {
            RuleFor(model => model.ApprenticeshipId).GreaterThan(ctx => 0).WithMessage("The Apprenticeship Id must be positive");
        }
    }
}
