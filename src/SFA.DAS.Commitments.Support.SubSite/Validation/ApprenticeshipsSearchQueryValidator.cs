using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;
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
            RuleFor(x => x)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Must(BeValidUlnNumber).WithMessage("You must enter a valid unique learner number")
                .Must(BeValidTenDigitUlnNumber).WithMessage("You must enter a 10-digit unique learner number");
        }

        private bool BeValidTenDigitUlnNumber(ApprenticeshipSearchQuery query)
        {
            if (query.SearchType != ApprenticeshipSearchType.SearchByUln)
            {
                return true;
            }

            var result = _ulnValidator.Validate(query.SearchTerm);

            if (result == UlnValidationResult.IsInValidTenDigitUlnNumber || result == UlnValidationResult.IsEmptyUlnNumber)
            {
                return false;
            }

            return true;
        }

        private bool BeValidUlnNumber(ApprenticeshipSearchQuery query)
        {
            if (query.SearchType != ApprenticeshipSearchType.SearchByUln)
            {
                return true;
            }

            if (_ulnValidator.Validate(query.SearchTerm) == UlnValidationResult.IsInvalidUln)
            {
                return false;
            }

            return true;
        }

    }
}