using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    

    public class ApprenticeshipStatusChangeCommandValidator : AbstractValidator<ApprenticeshipStatusChangeCommand>
    {
        public ApprenticeshipStatusChangeCommandValidator()
        {
            RuleFor(x => x.AccountId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            
        }
    }
   
}
