using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class ApprenticeshipResendInvitationCommandHandler(ILogger<ApprenticeshipResendInvitationCommandHandler> logger)
    : IHandleMessages<ApprenticeshipResendInvitationCommand>
{
    public async Task Handle(ApprenticeshipResendInvitationCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Forwarding SendApprenticeshipInvitationCommand to Apprentice Commitments for Apprenticeship {ApprenticeshipId}", message.ApprenticeshipId);
            
            await context.Send(new SendApprenticeshipInvitationCommand {CommitmentsApprenticeshipId = message.ApprenticeshipId, ResendOn = message.ResendOn});
        }
        catch (Exception)
        {
            logger.LogError("Forwarding SendApprenticeshipInvitationCommand failed");
            throw;
        }
    }
}