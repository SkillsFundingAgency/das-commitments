using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class AutomaticallyStopOverlappingTrainingDateRequestCommandHandler : IHandleMessages<AutomaticallyStopOverlappingTrainingDateRequestCommand>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler> _logger;


        public AutomaticallyStopOverlappingTrainingDateRequestCommandHandler(IMediator mediator,
            ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(AutomaticallyStopOverlappingTrainingDateRequestCommand message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation("Sending AutomaticallyStopOverlappingTrainingDateRequestCommand for ApprenticeshipId {apprenticeshipId}", message.ApprenticeshipId);

                await _mediator.Send(new StopApprenticeshipCommand(
                             message.AccountId,
                             message.ApprenticeshipId,
                             message.StopDate,
                             false,
                             Types.UserInfo.System,
                             Types.Party.Employer));
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Handling AutomaticallyStopOverlappingTrainingDateRequestCommand failed", ex);
                throw;
            }
        }
    }
}