using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQueryValidator : AbstractValidator<GetSupportApprenticeshipQuery>
    {
        public GetSupportApprenticeshipQueryValidator()
        {
            RuleFor(x => x.ApprenticeshipId).Must((x) =>
            {
                if (x.HasValue)
                {
                    return x.Value > 0;
                }

                return true;
            });
        }
    }
}