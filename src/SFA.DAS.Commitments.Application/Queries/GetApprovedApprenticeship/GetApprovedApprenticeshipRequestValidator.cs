using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship
{
    public class GetApprovedApprenticeshipRequestValidator : AbstractValidator<GetApprovedApprenticeshipRequest>
    {
        public GetApprovedApprenticeshipRequestValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.Caller).NotEmpty();

            Custom(request => request.Caller?.Id <= 0
                ? new FluentValidation.Results.ValidationFailure("CallerId", "CallerId must be greater than zero.")
                : null);
        }
    }
}
