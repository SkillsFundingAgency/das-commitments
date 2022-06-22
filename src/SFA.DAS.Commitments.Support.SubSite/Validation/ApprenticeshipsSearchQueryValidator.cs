using FluentValidation;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.Commitments.Support.SubSite.Validation
{
    public class ApprenticeshipsSearchQueryValidator : AbstractValidator<ApprenticeshipSearchQuery>
    {
        private readonly IUlnValidator _ulnValidator;

        public ApprenticeshipsSearchQueryValidator(IUlnValidator ulnValidator)
        {
            _ulnValidator = ulnValidator;
            ValidateUln();
        }

        protected void ValidateUln()
        {
            CascadeMode = CascadeMode.Stop;

            When(a => a.SearchType == ApprenticeshipSearchType.SearchByUln, () =>
            {
                RuleFor(x => x)
                               .Must((x) => (_ulnValidator.Validate(x.SearchTerm) != UlnValidationResult.IsInvalidUln)).WithMessage("Please enter a valid unique learner number")
                               .Must(BeValidTenDigitUlnNumber).WithMessage("Please enter a 10-digit unique learner number");
            });

            When(a => a.SearchType == ApprenticeshipSearchType.SearchByCohort, () =>
            {
                RuleFor(x => x)
                               .Must((x) => x.SearchTerm.Length == 6).WithMessage("Please enter a 6-digit Cohort number");
            });
        }

        private bool BeValidTenDigitUlnNumber(ApprenticeshipSearchQuery query)
        {
            var result = _ulnValidator.Validate(query.SearchTerm);
            return !(result == UlnValidationResult.IsInValidTenDigitUlnNumber || result == UlnValidationResult.IsEmptyUlnNumber);
        }
    }
}