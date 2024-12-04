using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

public class GetProviderPaymentsPriorityQueryValidator : AbstractValidator<GetProviderPaymentsPriorityQuery>
{
    public GetProviderPaymentsPriorityQueryValidator()
    {
        RuleFor(x => x.EmployerAccountId).GreaterThan(0).WithMessage("The employer account id must be supplied");
    }
}