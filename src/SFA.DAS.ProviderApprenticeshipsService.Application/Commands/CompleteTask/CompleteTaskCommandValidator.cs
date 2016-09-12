using FluentValidation;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CompleteTask
{
    public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
    {
        public CompleteTaskCommandValidator()
        {
            RuleFor(x => x.TaskId).GreaterThan(0);
            RuleFor(x => x.ProviderId).GreaterThan(0);
        }
    }
}