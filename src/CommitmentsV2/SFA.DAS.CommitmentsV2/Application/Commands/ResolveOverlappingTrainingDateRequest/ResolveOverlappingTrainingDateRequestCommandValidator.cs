using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest
{
    public class ResolveOverlappingTrainingDateRequestCommandValidator : AbstractValidator<ResolveOverlappingTrainingDateRequestCommand>
    {
        public ResolveOverlappingTrainingDateRequestCommandValidator()
        {
            RuleFor(model => model.ResolutionType).NotNull();
            RuleFor(model => model)
                .Must(x => x.ApprenticeshipId.GetValueOrDefault() > 0 || x.DraftApprenticeshipId.GetValueOrDefault() > 0)
                .WithMessage("ApprenticeshipId and DraftApprenticeshipId cannot be null");
        }
    }
}