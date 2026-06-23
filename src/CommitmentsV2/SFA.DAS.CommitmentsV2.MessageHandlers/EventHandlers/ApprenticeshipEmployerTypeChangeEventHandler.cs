using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;
using SFA.DAS.EmployerAccounts.Messages.Events;
using CommonEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;
using CommitmentsEmployerType = SFA.DAS.CommitmentsV2.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipEmployerTypeChangeEventHandler(IMediator mediator, ILogger<ApprenticeshipEmployerTypeChangeEventHandler> logger)
    : IHandleMessages<ApprenticeshipEmployerTypeChangeEvent>
{
    public Task Handle(ApprenticeshipEmployerTypeChangeEvent message, IMessageHandlerContext context)
    {
        if (message.ApprenticeshipEmployerType == CommonEmployerType.Unknown)
        {
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "ApprenticeshipEmployerTypeChangeEvent received for Account {AccountId} with type {ApprenticeshipEmployerType}",
            message.AccountId,
            message.ApprenticeshipEmployerType);

        var levyStatus = message.ApprenticeshipEmployerType == CommonEmployerType.Levy
            ? CommitmentsEmployerType.Levy
            : CommitmentsEmployerType.NonLevy;

        return mediator.Send(new UpdateAccountLevyStatusCommand
        {
            AccountId = message.AccountId,
            LevyStatus = levyStatus
        });
    }
}
