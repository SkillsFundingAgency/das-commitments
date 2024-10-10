using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

public class GetAccountLegalEntityQueryValidator :  AbstractValidator<GetAccountLegalEntityQuery>
{
    public GetAccountLegalEntityQueryValidator()
    {
        RuleFor(model => model.AccountLegalEntityId).Must(id => id > 0).WithMessage("The Account Legal Entity must be positive");
    }
}