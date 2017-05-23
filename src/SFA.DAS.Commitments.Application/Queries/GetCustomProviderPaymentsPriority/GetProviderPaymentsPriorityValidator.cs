using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityValidator : AbstractValidator<GetProviderPaymentsPriorityRequest>
    {
        public GetProviderPaymentsPriorityValidator()
        {
            RuleFor(x => x.EmployerAccountId).GreaterThan(0);
        }
    }
}
