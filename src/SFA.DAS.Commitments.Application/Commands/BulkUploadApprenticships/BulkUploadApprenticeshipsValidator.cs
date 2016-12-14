using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsValidator : AbstractValidator<BulkUploadApprenticeshipsCommand>
    {
        public BulkUploadApprenticeshipsValidator()
        {
            RuleFor(x => x.Apprenticeships).NotNull().SetCollectionValidator(new ApprenticeshipValidator());
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
