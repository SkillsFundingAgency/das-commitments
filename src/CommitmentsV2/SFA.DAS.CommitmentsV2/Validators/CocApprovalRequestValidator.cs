using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Validators;

public class CocApprovalRequestValidator :  AbstractValidator<CocApprovalRequest>
{
    public CocApprovalRequestValidator()
    {
        var learningTypes = EnumExtensions.GetEnumDescriptions<CocLearningType>();
        var fields = EnumExtensions.GetEnumDescriptions<CocChangeField>();

        RuleFor(model => model.LearningKey).GreaterThan(ctx => Guid.Empty).WithMessage("The LearningKey is not valid");
        RuleFor(model => model.ApprenticeshipId).GreaterThan(ctx => 0).WithMessage("The ApprenticeshipId must be greater than 0");
        RuleFor(model => model.UKPRN).NotNull().MaximumLength(10).WithMessage("The UKPRN not be more that {MaxLenth} characters");
        RuleFor(model => model.ULN).NotNull().MaximumLength(10).WithMessage("The ULN not be more that {MaxLenth} characters");
        RuleFor(model => model.AgreementId).GreaterThan(ctx => 0).WithMessage("The AgreementId must be greater than 0");
        RuleFor(model => model.LearningType).NotNull().Must(learningTypes.Contains).WithMessage("LearningType must be " + string.Join(", ", learningTypes[..^1]) + " or " + learningTypes[^1]);
        RuleFor(model => model.Changes).NotEmpty();
        RuleFor(x => x.Changes).Must(items => items == null || items.GroupBy(i => i.ChangeType, StringComparer.OrdinalIgnoreCase).All(g => g.Count() == 1)).WithMessage("ChangeType must be unique within the listed values.");
        RuleFor(x => x.Changes).Must(list => list.All(p => fields.Contains(p.ChangeType))).WithMessage("ChangeType must be " + fields.First() + " or " + fields.Last());
        RuleFor(x => x.Changes).Must(list => list.All(p => p.Data != null)).WithMessage("ChangeType must contain a Data structure");
    }
}