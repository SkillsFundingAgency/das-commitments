using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        protected  void ValidateUln()
        {
            When(a => a.SearchType == ApprenticeshipSearchType.SearchByUln, () =>
            {
                RuleFor(x => x)
                               .Cascade(CascadeMode.StopOnFirstFailure)
                               .Must(BeValidUlnNumber).WithMessage("Please enter a valid unique learner number")
                               .Must(BeValidTenDigitUlnNumber).WithMessage("Please enter a 10-digit unique learner number");
            });

            When(a => a.SearchType == ApprenticeshipSearchType.SearchByCohort, () =>
            {
                RuleFor(x => x)
                               .Cascade(CascadeMode.StopOnFirstFailure)
                               .Must(BeValidCohortNumber).WithMessage("Please enter a 6-digit Cohort number");
            });
        }
        private bool BeValidTenDigitUlnNumber(ApprenticeshipSearchQuery query)
        {
            var result = _ulnValidator.Validate(query.SearchTerm);

            if (result == UlnValidationResult.IsInValidTenDigitUlnNumber || result == UlnValidationResult.IsEmptyUlnNumber)
            {
                return false;
            }

            return true;
        }
        private bool BeValidUlnNumber(ApprenticeshipSearchQuery query)
        {
            if (_ulnValidator.Validate(query.SearchTerm) == UlnValidationResult.IsInvalidUln)
            {
                return false;
            }

            return true;
        }

        private bool BeValidCohortNumber(ApprenticeshipSearchQuery query)
        {
            if(query.SearchTerm.Length != 6)
            {
                return false;
            }

            return true;
        }
    }
}