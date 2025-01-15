using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Exceptions;
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
        catch (BadRequestException ex) when
            (ex.Message.StartsWith("Apprenticeship") && ex.Message.EndsWith("not found"))
        {
            logger.LogError(ex, "Handling AutomaticallyStopOverlappingTrainingDateRequestCommand not processed as apprenticeship not found");
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Handling AutomaticallyStopOverlappingTrainingDateRequestCommand not processed because a domain exception was thrown");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Handling AutomaticallyStopOverlappingTrainingDateRequestCommand failed");
            throw;
        }
    }
}