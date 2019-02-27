using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Queries.GetEmployer
{
    public class GetEmployerValidator :  AbstractValidator<GetEmployerRequest>
    {
        public GetEmployerValidator()
        {
            RuleFor(model => model.AccountLegalEntityId).Must(id => id > -1).WithMessage("The Account Legal Entity must be positive");
        }
    }
}
