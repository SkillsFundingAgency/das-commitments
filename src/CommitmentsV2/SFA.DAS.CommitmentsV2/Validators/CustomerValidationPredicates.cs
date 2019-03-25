using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public static class CustomerValidationExtensions   
    {
        public static IRuleBuilderOptions<T, int?> CostMustBeValid<T>(this IRuleBuilder<T, int?> ruleBuilder)
        {
            return ruleBuilder.CostMustBeBetween(0, 100000);
        }

        public static IRuleBuilderOptions<T, int?> CostMustBeBetween<T>(this IRuleBuilder<T, int?> ruleBuilder, int minCost, int maxCost)
        {
            return ruleBuilder
                .Must(cost => cost.HasValue).WithMessage("A value must be supplied for {PropertyName}")
                .Must((rootObject, cost, context) =>
                {
                    context.MessageFormatter.AppendArgument("MinCost", minCost);
                    context.MessageFormatter.AppendArgument("MaxCost", maxCost);
                    return cost.HasValue && cost.Value >= minCost && cost.Value <= maxCost;
                })
                .WithMessage("{PropertyValue} is invalid for {PropertyName} - it must be between {MinCost} and {MaxCost}");
        }

        public static IRuleBuilderOptions<T, string> ReferenceMustBeValidIfSupplied<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.ReferenceMustBeValidIfSupplied(1, 20);
        }

        public static IRuleBuilderOptions<T, string> ReferenceMustBeValidIfSupplied<T>(this IRuleBuilder<T, string> ruleBuilder, int minLength, int maxLength)
        {
            return ruleBuilder
                .Must((rootObject, reference, context) =>
                {
                    context.MessageFormatter.AppendArgument("MinLength", minLength);
                    context.MessageFormatter.AppendArgument("MaxLength", maxLength);
                    return string.IsNullOrWhiteSpace(reference) || (reference.Length >= minLength && reference.Length <= maxLength);
                })
                .WithMessage("{PropertyValue} is invalid for {PropertyName} - it must be between {MinLength} and {MaxLength} characters");
        }
    }
}