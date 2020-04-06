using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest
{
    public class ChangeOfPartyRequestValidator : AbstractValidator<ChangeOfPartyRequestCommand>
    {
        public ChangeOfPartyRequestValidator()
        {
            RuleFor(model => model.NewPrice).NotNull().WithMessage("The NewPrice cannot be empty");
            RuleFor(model => model.NewStartDate).NotNull().WithMessage("The NewStartDate cannot be empty");
            RuleFor(model => model.UserInfo).NotNull().WithMessage("The UserInfo cannot be empty");
            RuleFor(model => model.PartyId).Must(id => id > 0).WithMessage("The PartyId must be positive");
            RuleFor(model => model.ApprenticeshipId).Must(id => id > 0).WithMessage("The ApprenticeshipId must be positive");
        }
    }
}
