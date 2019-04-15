using FluentValidation;
using FluentValidation.Validators;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using TrainingProgrammeStatus = SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class DraftApprenticeshipDetailsValidator : AbstractValidator<DraftApprenticeshipDetails>
    {
        private class StartDateValidator : AbstractValidator<DraftApprenticeshipDetails>
        {
            public StartDateValidator(IAcademicYearDateProvider academicYearDateProvider)
            {
                CascadeMode = CascadeMode.StopOnFirstFailure;

                RuleFor(ctx => ctx.StartDate)
                    .GreaterThanOrEqualTo(Constants.DasStartDate)
                    .When(ctx => ctx.TrainingProgramme == null || ctx.TrainingProgramme.IsActiveOn(ctx.StartDate))
                    .WithMessage($"The start date must not be earlier than {Constants.DasStartDate:MM yyyy}");

                  RuleFor(ctx => ctx.StartDate)
                            .Must((draftApprenticeship, startDate) =>
                                draftApprenticeship.TrainingProgramme.IsActiveOn(draftApprenticeship.StartDate))
                            .When(ctx => ctx.TrainingProgramme != null)
                            .WithMessage(draftApprenticeship =>
                            {
                                var suffix =
                                    draftApprenticeship.TrainingProgramme.GetStatusOn(draftApprenticeship.StartDate
                                        .Value) ==
                                    TrainingProgrammeStatus.Pending
                                        ? $"after {draftApprenticeship.TrainingProgramme.EffectiveFrom.Value.AddMonths(-1):MM yyyy}"
                                        : $"before {draftApprenticeship.TrainingProgramme.EffectiveTo.Value.AddMonths(1):MM yyyy}";

                                return
                                    $"This training course is only available to apprentices with a start date {suffix}";
                            });

                    RuleFor(ctx => ctx.StartDate)
                        .LessThanOrEqualTo(draftApprenticeship =>
                            academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
                        .WithMessage(
                            "The start date must be no later than one year after the end of the current teaching year");
            }
        }

        private class EndDateValidator : AbstractValidator<DraftApprenticeshipDetails>
        {
            public EndDateValidator(ICurrentDateTime currentDateTime) 
            {
                RuleFor(ctx => ctx.EndDate)
                    .Must(endDate => endDate > currentDateTime.UtcToday)
                    .WithMessage("The end date must not be in the past");

                RuleFor(ctx => ctx.EndDate)
                    .Must((ctx, endDate) => endDate >= ctx.StartDate)
                    .When(ctx => ctx.StartDate.HasValue)
                    .WithMessage("The end date must not be on or before the start date");
            }
        }

        private class DateOfBirthValidator : AbstractValidator<DraftApprenticeshipDetails>
        {
            public DateOfBirthValidator(ICurrentDateTime currentDateTime)
            {
                RuleFor(ctx => ctx.DateOfBirth)
                    .Must(dateOfBirth =>
                        dateOfBirth.Value.Age(currentDateTime.UtcNow) >= Constants.MinimumAgeAtApprenticeshipStart)
                    .When(ctx => ctx.DateOfBirth.HasValue)
                    .WithMessage(
                        $"The apprentice must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training");

                RuleFor(ctx => ctx.DateOfBirth)
                    .Must(dateOfBirth =>
                        dateOfBirth.Value.Age(currentDateTime.UtcNow) <= Constants.MaximumAgeAtApprenticeshipStart)
                    .When(ctx => ctx.DateOfBirth.HasValue)
                    .WithMessage(
                        $"The apprentice must be younger than {Constants.MaximumAgeAtApprenticeshipStart + 1} years old at the start of their training");
            }
        }

        private class CostValidator : AbstractValidator<DraftApprenticeshipDetails>
        {
            public CostValidator()
            {
                RuleFor(ctx => ctx.Cost)
                    .GreaterThan(0)
                    .WithMessage("Enter the total agreed training cost");

                RuleFor(ctx => ctx.Cost)
                    .LessThanOrEqualTo(100000)
                    .WithMessage("The total cost must be £100,000 or less");
            }
        }

        public DraftApprenticeshipDetailsValidator(
            IUlnValidator ulnValidator,
            ICurrentDateTime currentDateTime,
            IAcademicYearDateProvider academicYearDateProvider)
        {
            RuleFor(ctx => ctx.FirstName)
                .NotEmpty()
                .WithMessage("First name must be entered");

            RuleFor(ctx => ctx.FirstName)
                    .Must(firstName => firstName.Length <= 100)
                    .When(ctx => !string.IsNullOrWhiteSpace(ctx.FirstName))
                    .WithMessage("You must enter a first name that's no longer than 100 characters");

            RuleFor(ctx => ctx.LastName)
                .NotEmpty()
                .WithMessage("Last name must be entered");

            RuleFor(ctx => ctx.LastName)
                .Must(lastName => lastName.Length <= 100)
                .When(ctx => !string.IsNullOrWhiteSpace(ctx.LastName))
                .WithMessage("You must enter a last name that's no longer than 100 characters");

            RuleFor(ctx => ctx)
                .SetValidator(new EndDateValidator(currentDateTime))
                .When(ctx => ctx.EndDate.HasValue);

            RuleFor(ctx => ctx)
                .SetValidator(new CostValidator());

            RuleFor(ctx => ctx.Reference)
                .Must(reference => reference.Length <= 20)
                .When(ctx => !string.IsNullOrEmpty(ctx.Reference))
                .WithMessage("The Reference must be 20 characters or fewer");

            RuleFor(ctx => ctx.Uln)
                .Must((obj, uln, ctx) => ValidUln(uln, ctx, ulnValidator))
                .When(ctx => !string.IsNullOrWhiteSpace(ctx.Uln))
                .WithMessage("You must enter {rule} unique learner number");

            RuleFor(ctx => ctx)
                .SetValidator(new DateOfBirthValidator(currentDateTime))
                .When(ctx => ctx.DateOfBirth.HasValue);

            RuleFor(ctx => ctx)
                .SetValidator(new StartDateValidator(academicYearDateProvider))
                .When(ctx => ctx.StartDate.HasValue);
        }

        private bool ValidUln(string uln, PropertyValidatorContext ctx, IUlnValidator ulnValidator)
        {
            var validationResult = ulnValidator.Validate(uln);
            switch (validationResult)
            {
                case UlnValidationResult.IsInValidTenDigitUlnNumber:
                    ctx.MessageFormatter.AppendArgument("rule", "a 10-digit");
                    return false;

                case UlnValidationResult.IsInvalidUln:
                    ctx.MessageFormatter.AppendArgument("rule", "a valid");
                    return false;
            }

            return true;
        }
    }
}