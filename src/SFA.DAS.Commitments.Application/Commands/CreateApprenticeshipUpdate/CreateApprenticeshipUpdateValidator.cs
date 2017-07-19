using FluentValidation;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    public class CreateApprenticeshipUpdateValidator : AbstractValidator<CreateApprenticeshipUpdateCommand>
    {
        public CreateApprenticeshipUpdateValidator()
        {
            RuleFor(x => x.ApprenticeshipUpdate.Originator).NotNull();
            RuleFor(x => x.ApprenticeshipUpdate.ApprenticeshipId).NotEmpty();

            RuleFor(x => x.ApprenticeshipUpdate.EmployerRef)
                .Must(x => x == null)
                .When(x => x.ApprenticeshipUpdate.Originator == Originator.Provider)
                .WithMessage("Provider cannot modify EmployerRef");

            RuleFor(x => x.ApprenticeshipUpdate.ProviderRef)
                .Must(x => x == null)
                .When(x => x.ApprenticeshipUpdate.Originator == Originator.Employer)
                .WithMessage("Employer cannot modify ProviderRef");

            RuleFor(x => x.ApprenticeshipUpdate.ULN)
                .Must(x => x == null)
                .When(x => x.ApprenticeshipUpdate.Originator == Originator.Employer)
                .WithMessage("Employer cannot modify ULN");

            RuleFor(x => x.ApprenticeshipUpdate.Originator)
                .Must(x => x == Originator.Provider)
                .When(x => x.Caller.CallerType == CallerType.Provider);

            RuleFor(x => x.ApprenticeshipUpdate.Originator)
                .Must(x => x == Originator.Employer)
                .When(x => x.Caller.CallerType == CallerType.Employer);

            RuleFor(x => x.ApprenticeshipUpdate.ULN)
                .Empty()
                .When(x => x.ApprenticeshipUpdate.Originator == Originator.Employer);

            RuleFor(x => x.ApprenticeshipUpdate).Must(
                x => x.FirstName != null
                || x.LastName != null
                || x.DateOfBirth != null
                || x.ULN != null
                || x.TrainingType != null
                || x.TrainingCode != null
                || x.TrainingName != null
                || x.Cost != null
                || x.StartDate != null
                || x.EndDate != null
                || x.ProviderRef != null
                || x.EmployerRef != null
            )
            .WithMessage("At least one data field must be present");
        }
    }
}
