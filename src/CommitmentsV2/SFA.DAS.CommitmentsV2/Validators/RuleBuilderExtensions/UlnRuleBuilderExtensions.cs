using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Validators.RuleBuilderExtensions;

public static class UlnRuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeValidUln<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        IUlnValidator ulnValidator)
    {
        return ruleBuilder
            .Must(uln => ulnValidator.Validate(uln) == UlnValidationResult.Success)
            .WithMessage("The ULN is invalid");
    }
}
