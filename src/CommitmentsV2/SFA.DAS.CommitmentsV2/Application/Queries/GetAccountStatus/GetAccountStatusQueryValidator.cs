using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;

public class GetAccountStatusQueryValidator : AbstractValidator<GetAccountStatusQuery>
{
    public GetAccountStatusQueryValidator()
    {
        RuleFor(model => model.AccountId).Must(id => id > 0);
    }
}