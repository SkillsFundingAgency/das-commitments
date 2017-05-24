using FluentValidation;
using SFA.DAS.Commitments.Domain.Entities;
using System;
using System.Collections.Generic;
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
                .Must(ProviderIdsAreValidValues()).WithMessage("Provier Ids are not valid values")
                .Must(ProviderIdsAreUnique()).WithMessage("Provider Ids must be unique")
                .Must(PriorityValuesAreUnique()).WithMessage("Priority values must be unique")
                .Must(PriorityValuesSequentialFromOne()).WithMessage("Priority values must be sequestial starting from 1");
        }

        private static Func<List<ProviderPaymentPriorityUpdateItem>, bool> PriorityValuesSequentialFromOne()
        {
            return x =>
            {
                var range = Enumerable.Range(1, x.Count);

                var matchCount = x.Select(y => y.PriorityOrder).Intersect(range).Count();

                return matchCount == x.Count;
            };
        }

        private static Func<List<ProviderPaymentPriorityUpdateItem>, bool> PriorityValuesAreUnique()
        {
            return x =>
            {
                var count = x
                    .Select(y => y.PriorityOrder).Count();

                var distinct = x
                    .Select(y => y.PriorityOrder)
                    .Distinct().Count();

                return count == distinct;
            };
        }

        private static Func<List<ProviderPaymentPriorityUpdateItem>, bool> ProviderIdsAreValidValues()
        {
            return x =>
            {
                return !x.Any(y => y.ProviderId == 0);
            };
        }

        private static Func<List<ProviderPaymentPriorityUpdateItem>, bool> ProviderIdsAreUnique()
        {
            return x =>
            {
                var count = x
                    .Select(y => y.ProviderId).Count();

                var distinct = x
                    .Select(y => y.ProviderId)
                    .Distinct().Count();

                return count == distinct;
            };
        }
    }
}
