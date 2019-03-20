﻿using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortValidator :  AbstractValidator<AddCohortCommand>
    {
        public AddCohortValidator()
        {
            RuleFor(model => model.AccountLegalEntityId).Must(id => id > 0).WithMessage("The Account Legal Entity must be positive");
            RuleFor(model => model.ProviderId).Must(id => id > 0).WithMessage("The UkPrn must be positive");
        }
    }
}
