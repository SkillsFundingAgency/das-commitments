using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest
{
    public class ResolveOverlappingTrainingDateRequestCommandValidator : AbstractValidator<ResolveOverlappingTrainingDateRequestCommand>
    {
        public ResolveOverlappingTrainingDateRequestCommandValidator()
        {
            RuleFor(model => model.ResolutionType).NotNull();
            RuleFor(model => model)
                .Must(x => x.ApprenticeshipId.GetValueOrDefault() > 0)
                .WithMessage("Apprenticeship Id must be greater than zero");
        }
    }
}