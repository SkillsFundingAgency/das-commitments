using System;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ApprenticeshipResendInvitationCommandHandler : IHandleMessages<ApprenticeshipResendInvitationCommand>
    {
        private readonly ILogger<ApprenticeshipResendInvitationCommandHandler> _logger;

        public ApprenticeshipResendInvitationCommandHandler(ILogger<ApprenticeshipResendInvitationCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipResendInvitationCommand message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation(
                    "Forwarding SendApprenticeshipInvitationCommand to Apprentice Commitments for Apprenticeship {0}",
                    message.ApprenticeshipId);
                await context.Send(new SendApprenticeshipInvitationCommand
                    {CommitmentsApprenticeshipId = message.ApprenticeshipId, ResendOn = message.ResendOn});
            }
            catch (Exception)
            {
                _logger.LogError("Forwarding SendApprenticeshipInvitationCommand failed");
                throw;
            }
        }
    }
}
