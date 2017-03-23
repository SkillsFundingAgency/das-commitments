using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    public class CreateApprenticeshipUpdateValidator : AbstractValidator<CreateApprenticeshipUpdateCommand>
    {
        public CreateApprenticeshipUpdateValidator()
        {
            RuleFor(x => x.ApprenticeshipUpdate.Originator).NotNull();
            RuleFor(x => x.ApprenticeshipUpdate.ApprenticeshipId).NotEmpty();

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
            )
            .WithMessage("At least one data field must be present");
        }
    }
}
