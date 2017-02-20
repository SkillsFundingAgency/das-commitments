using FluentValidation;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipValidator : AbstractValidator<UpdateApprenticeshipCommand>
    {
        public UpdateApprenticeshipValidator(AbstractValidator<Apprenticeship> apprenticeshipValidator)
        {
            RuleFor(x => x.Apprenticeship).NotNull().SetValidator(apprenticeshipValidator);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);

            Custom(request =>
            {
                switch (request.Caller.CallerType)
                {
                    case CallerType.Provider:
                        if (request.Caller.Id <= 0)
                            return new FluentValidation.Results.ValidationFailure("ProviderId", "ProviderId must be greater than zero.");
                        break;
                    case CallerType.Employer:
                    default:
                        if (request.Caller.Id <= 0)
                            return new FluentValidation.Results.ValidationFailure("AccountId", "AccountId must be greater than zero.");
                        break;
                }

                return null;
            });
        }
    }
}
