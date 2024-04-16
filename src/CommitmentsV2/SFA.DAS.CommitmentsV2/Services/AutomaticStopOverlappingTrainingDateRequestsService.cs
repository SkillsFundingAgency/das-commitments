using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using IMessageSession = NServiceBus.IMessageSession;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AutomaticStopOverlappingTrainingDateRequestsService : IAutomaticStopOverlappingTrainingDateRequestsService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AutomaticStopOverlappingTrainingDateRequestsService> _logger;
        private readonly IMessageSession _messageSession;

        public AutomaticStopOverlappingTrainingDateRequestsService(IMediator mediator,
            IMessageSession messageSession,
            ILogger<AutomaticStopOverlappingTrainingDateRequestsService> logger)
        {
            _mediator = mediator;
            _messageSession = messageSession;
            _logger = logger;
        }

        public async Task AutomaticallyStopOverlappingTrainingDateRequests()
        {
            try
            {
                var overlappingTrainingDatesToStop = await _mediator.Send(new GetPendingOverlappingTrainingDatesToStopQuery());
                if (overlappingTrainingDatesToStop != null && overlappingTrainingDatesToStop.OverlappingTrainingDateRequests != null)
                {
                    foreach (var request in overlappingTrainingDatesToStop.OverlappingTrainingDateRequests)
                    {
                        if (request.DraftApprenticeship != null)
                        {
                            _logger.LogInformation("Sending StopApprenticeshipRequest for ApprenticeshipId {PreviousApprenticeshipId}", request.PreviousApprenticeshipId);

                            await _messageSession.Send(new AutomaticallyStopOverlappingTrainingDateRequestCommand(
                                request.PreviousApprenticeship.Cohort.EmployerAccountId,
                                request.PreviousApprenticeshipId,
                                request.DraftApprenticeship.StartDate.Value,
                                false,
                                Types.UserInfo.System,
                                Types.Party.Employer));
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("An error occurred while automatically stopping overlapping training date requests.", ex);

                throw;
            }
        }
    }
}
