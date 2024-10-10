using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class AutomaticallyStopOverlappingTrainingDateRequestCommandHandler(
    IMediator mediator,
    ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler> logger)
    : IHandleMessages<AutomaticallyStopOverlappingTrainingDateRequestCommand>
{
    public async Task Handle(AutomaticallyStopOverlappingTrainingDateRequestCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Sending AutomaticallyStopOverlappingTrainingDateRequestCommand for ApprenticeshipId {ApprenticeshipId}", message.ApprenticeshipId);

            await mediator.Send(new StopApprenticeshipCommand(
                message.AccountId,
                message.ApprenticeshipId,
                message.StopDate,
                false,
                Types.UserInfo.System,
                Types.Party.Employer));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Handling AutomaticallyStopOverlappingTrainingDateRequestCommand failed");
            throw;
        }
    }
}