using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipValidator : AbstractValidator<GetApprenticeshipRequest>
    {
        public GetApprenticeshipValidator()
        {
            Custom(request =>
            {
                if (request.ProviderId.HasValue && request.AccountId.HasValue)
                    return new FluentValidation.Results.ValidationFailure("ProviderId/AccountId", "ProviderId and AccountId cannot both have a value.");

                if (!request.ProviderId.HasValue && !request.AccountId.HasValue)
                    return new FluentValidation.Results.ValidationFailure("ProviderId/AccountId", "ProviderId or AccountId must have a value.");

                if (request.ProviderId.HasValue && request.ProviderId <= 0)
                    return new FluentValidation.Results.ValidationFailure("ProviderId", "ProviderId must be greater than zero.");

                if (request.AccountId.HasValue && request.AccountId <= 0)
                    return new FluentValidation.Results.ValidationFailure("AccountId", "AccountId must be greater than zero.");

                return null;
            });
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
        }
    }
}
