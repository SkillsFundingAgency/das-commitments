using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;

public class GetApprenticeshipLockedFieldsQueryValidator : AbstractValidator<GetApprenticeshipLockedFieldsQuery>
{
    public GetApprenticeshipLockedFieldsQueryValidator()
    {
        RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
    }
}