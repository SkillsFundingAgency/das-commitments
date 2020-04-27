using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest
{
    public class CreateChangeOfPartyRequestValidator : AbstractValidator<CreateChangeOfPartyRequestCommand>
    {
        public CreateChangeOfPartyRequestValidator()
        {
            RuleFor(model => model.UserInfo).NotNull().WithMessage("The UserInfo cannot be empty");
            RuleFor(model => model.NewPartyId).Must(id => id > 0).WithMessage("The NewPartyId must be positive");
            RuleFor(model => model.ApprenticeshipId).Must(id => id > 0).WithMessage("The ApprenticeshipId must be positive");
        }
    }
}
