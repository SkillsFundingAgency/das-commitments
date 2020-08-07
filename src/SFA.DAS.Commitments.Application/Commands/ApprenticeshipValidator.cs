using System;
using System.Text.RegularExpressions;
using FluentValidation;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;

namespace SFA.DAS.Commitments.Application.Commands
{
    public sealed class ApprenticeshipValidator : AbstractValidator<Apprenticeship>
    {
        private static Func<string, int, bool> _lengthLessThanFunc = (str, length) => (str?.Length ?? length) < length;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IUlnValidator _ulnValidator;
        IAcademicYearValidator _academicYearValidator;

        public ApprenticeshipValidator(ICurrentDateTime currentDate, IUlnValidator ulnValidator, IAcademicYearValidator academicYearValidator)
        {
            _currentDateTime = currentDate;
            _ulnValidator = ulnValidator;
            _academicYearValidator = academicYearValidator;

            ValidateFirstName();
            ValidateLastName();
            ValidateUln();
            ValidateDateOfBirth();
            ValidateTrainingCodes();
            ValidateStartDate();
            ValidateEndDate();
            ValidateCost();
            ValidateNino();
            ValidateProviderReference();
            ValidateEmployerReference();
        }

        private void ValidateFirstName()
        {
            RuleFor(x => x.FirstName).NotEmpty().Must(m => _lengthLessThanFunc(m, 101));
        }

        private void ValidateLastName()
        {
            RuleFor(x => x.LastName).NotEmpty().Must(m => _lengthLessThanFunc(m, 101));
        }

        private void ValidateUln()
        {
            RuleFor(x => x.ULN).Must(BeValidUlnNumber);
        }

        private void ValidateDateOfBirth()
        {
            When(x => x.DateOfBirth != null && x.StartDate != null, () =>
            {
                RuleFor(x => x.DateOfBirth).Must((apprenticship, dob) =>
                {
                    return WillApprenticeBeAtLeast15AtStartOfTraining(apprenticship, dob);
                });
            });
        }

        private void ValidateTrainingCodes()
        {
            When(x => !string.IsNullOrWhiteSpace(x.TrainingCode) || !string.IsNullOrWhiteSpace(x.TrainingName), () =>
            {
                RuleFor(x => x.TrainingType).IsInEnum();
                RuleFor(x => x.TrainingCode).NotEmpty();
                RuleFor(x => x.TrainingName).NotEmpty();

                When(x => x.TrainingType == TrainingType.Framework, () =>
                {
                    RuleFor(x => x.TrainingCode).Matches(@"^[1-9]\d{0,2}-[1-9]\d{0,1}-[1-9]\d{0,2}$");
                });

                When(x => x.TrainingType == TrainingType.Standard, () =>
                {
                    RuleFor(x => x.TrainingCode).Must(x =>
                    {
                        int code;
                        if (!int.TryParse(x, out code))
                            return false;

                        return code > 0;
                    });
                });
            });
        }

        private void ValidateStartDate()
        {
            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(new DateTime(2017, 5, 1)).Unless(x => x.StartDate == null);
        }

        private void ValidateEndDate()
        {
            RuleFor(r => r.EndDate)
                                .Must(BeGreaterThenStartDate);
        }

        private void ValidateCost()
        {
            RuleFor(x => x.Cost).Must(CostIsValid);
        }

        private void ValidateNino()
        {
            RuleFor(x => x.NINumber)
                            .Matches(@"^[abceghj-prstw-z][abceghj-nprstw-z]\d{6}[abcd\s]$", RegexOptions.IgnoreCase)
                                .Unless(m => m.NINumber == null);
        }

        private void ValidateProviderReference()
        {
            RuleFor(x => x.ProviderRef).Length(0, 20);
        }
        
        private void ValidateEmployerReference()
        {
            RuleFor(x => x.EmployerRef).Length(0, 20);
        }

        private static bool WillApprenticeBeAtLeast15AtStartOfTraining(Apprenticeship apprenticship, DateTime? dob)
        {
            DateTime startDate = apprenticship.StartDate.Value;
            DateTime dobDate = dob.Value;
            int age = startDate.Year - dobDate.Year;
            if (startDate < dobDate.AddYears(age)) age--;

            return age >= 15;
        }

        private bool CostIsValid(decimal? cost)
        {
            if (cost.HasValue)
            {
                if (cost <= 0 || HasGreaterThan2DecimalPlaces(cost.Value) || cost.Value > 100000)
                    return false;
            }

            return true;
        }

        private static bool HasGreaterThan2DecimalPlaces(decimal cost)
        {
            return decimal.GetBits(cost)[3] >> 16 > 2;
        }

        private bool BeGreaterThenStartDate(Apprenticeship viewModel, DateTime? date)
        {
            if (viewModel.StartDate == null || viewModel.EndDate == null) return true;

            return viewModel.StartDate < viewModel.EndDate;
        }

        private bool BeValidUlnNumber(string uln)
        {
            var result = _ulnValidator.Validate(uln);
            return (result == UlnValidationResult.Success || result == UlnValidationResult.IsEmptyUlnNumber);
        }

    }
}
