using FluentValidation;
using System.Linq;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority
{
    public sealed class UpdateProviderPaymentsPriorityCommandValidator : AbstractValidator<UpdateProviderPaymentsPriorityCommand>
    {
        public UpdateProviderPaymentsPriorityCommandValidator()
        {
            RuleFor(x => x.EmployerAccountId).GreaterThan(0);

            RuleFor(x => x.ProviderPriorities)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Must(x =>
                {
                    var count = x.Count;
                    var distinct = x.Distinct().Count();

                    return count == distinct;
                })
                .Must(x =>
                {
                    return !x.Any(y => y == 0);
                });
        }
    }
}
