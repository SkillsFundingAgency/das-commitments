using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

public class GetDraftApprenticeshipsQueryValidator : AbstractValidator<GetDraftApprenticeshipsQuery>
{
    public GetDraftApprenticeshipsQueryValidator()
    {
        RuleFor(model => model.CohortId).GreaterThan(0);
    }
}