using FluentValidation;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class CreateChangeOfPartyRequestValidator : AbstractValidator<CreateChangeOfPartyRequestCommand>
{
    private readonly IAcademicYearDateProvider _academicYearDateProvider;
    public CreateChangeOfPartyRequestValidator(IAcademicYearDateProvider academicYearDateProvider)
    {
        _academicYearDateProvider = academicYearDateProvider;

        RuleFor(model => model.UserInfo).NotNull().WithMessage("The UserInfo cannot be empty");
        RuleFor(model => model.NewPartyId).Must(id => id > 0).WithMessage("The NewPartyId must be positive");
        RuleFor(model => model.ApprenticeshipId).Must(id => id > 0).WithMessage("The ApprenticeshipId must be positive");

        When(model => model.NewPrice.HasValue || model.NewStartDate.HasValue || model.NewEndDate.HasValue, () =>
        {
            RuleFor(model => model.NewPrice).NotNull().WithMessage("The NewPrice cannot be null if the NewStartDate or NewEndDate have values");
            RuleFor(model => model.NewStartDate).NotNull().WithMessage("The NewStartDate cannot be null if the NewPrice or NewEndDate have values");
            RuleFor(model => model.NewEndDate).NotNull().WithMessage("The NewEndDate cannot be null if the NewStartDate or NewPrice have values");
        });

        When(model => model.NewPrice.HasValue, () =>
        {
            RuleFor(model => model.NewPrice).GreaterThan(0).WithMessage("The NewPrice must be greater than 0");
            RuleFor(model => model.NewPrice).LessThanOrEqualTo(100000).WithMessage("The NewPrice must be 100000 or less");
        });

        When(model => model.NewStartDate.HasValue && model.NewEndDate.HasValue, () =>
        {
            RuleFor(model => model.NewStartDate).Must((args, value) => value.Value < args.NewEndDate)
                .WithMessage("The NewStartDate must be before the NewEndDate");

            RuleFor(model => model.NewStartDate).Must(newStartDate => newStartDate.Value <= _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
                .WithMessage("The start date must be no later than one year after the end of the current teaching year");
        });
    }
}