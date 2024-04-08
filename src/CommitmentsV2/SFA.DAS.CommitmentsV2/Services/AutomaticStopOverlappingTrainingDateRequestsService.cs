using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AutomaticStopOverlappingTrainingDateRequestsService : IAutomaticStopOverlappingTrainingDateRequestsService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AutomaticStopOverlappingTrainingDateRequestsService> _logger;

        public AutomaticStopOverlappingTrainingDateRequestsService(IMediator mediator, ILogger<AutomaticStopOverlappingTrainingDateRequestsService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task AutomaticallyStopOverlappingTrainingDateRequest()
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

                            await _mediator.Send(new StopApprenticeshipCommand(
                                request.PreviousApprenticeship.Cohort.EmployerAccountId,
                                request.PreviousApprenticeshipId,
                                request.DraftApprenticeship.StartDate.Value,
                                false,
                                Types.UserInfo.System));
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
