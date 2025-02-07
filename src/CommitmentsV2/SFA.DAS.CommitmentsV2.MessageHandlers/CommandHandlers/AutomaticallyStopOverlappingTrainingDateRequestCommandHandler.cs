using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

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
            var apprenticeship = await mediator.Send(new GetApprenticeshipQuery(message.ApprenticeshipId));
            if (apprenticeship == null)
            {
                logger.LogInformation("Handling AutomaticallyStopOverlappingTrainingDateRequestCommand not processed as apprenticeship was not found");
                return;
            }

            if (apprenticeship.Status == ApprenticeshipStatus.Completed || apprenticeship.Status == ApprenticeshipStatus.Stopped)
            {
                logger.LogInformation("Handling AutomaticallyStopOverlappingTrainingDateRequestCommand not processed as apprenticeship was already completed or stopped");
                return;
            }

            logger.LogInformation(
                "Sending AutomaticallyStopOverlappingTrainingDateRequestCommand for ApprenticeshipId {ApprenticeshipId}",
                message.ApprenticeshipId);

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