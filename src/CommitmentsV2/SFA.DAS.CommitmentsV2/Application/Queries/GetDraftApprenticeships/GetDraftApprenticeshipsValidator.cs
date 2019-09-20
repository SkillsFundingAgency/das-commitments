using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsValidator : AbstractValidator<GetDraftApprenticeshipsRequest>
    {
        public GetDraftApprenticeshipsValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0);
        }
    }
}
