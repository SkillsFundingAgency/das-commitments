using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice
{
    public class GetDraftApprenticeValidator :  AbstractValidator<GetDraftApprenticeRequest>
    {
        public GetDraftApprenticeValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
        }
    }
}
