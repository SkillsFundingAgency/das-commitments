using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class CohortDeletedWithPendingOverlappingTrainingDateEventHandler : IHandleMessages<CohortDeletedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<CohortDeletedWithPendingOverlappingTrainingDateEventHandler> _logger;
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public CohortDeletedWithPendingOverlappingTrainingDateEventHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService,
            ILogger<CohortDeletedWithPendingOverlappingTrainingDateEventHandler> logger)
        {
            _dbContext = dbContext;
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
            _logger = logger;
        }
        public async Task Handle(CohortDeletedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("CohortDeletedEvent received for Cohort {cohortId}, with pending OverlappingTrainingDateRequest", message.CohortId);

            try
            {
                var cohort = await _dbContext.Value.Cohorts
                    .Include(x => x.Apprenticeships)
                    .Where(x => x.Id == message.CohortId)
                    .FirstOrDefaultAsync();

                if (cohort == null)
                {
                    _logger.LogWarning("No cohort found for Id {cohortId}", message.CohortId);
                    return;
                }

                var overlappingTrainingDateRequest = cohort.Apprenticeships
                    .Where(apprenticeship => apprenticeship.OverlappingTrainingDateRequests != null)
                    .OrderByDescending(apprenticeship => apprenticeship.CreatedOn)
                    .SelectMany(apprenticeship => apprenticeship.OverlappingTrainingDateRequests)
                    .OrderByDescending(request => request.Id)
                    .FirstOrDefault();

                if (overlappingTrainingDateRequest == null)
                {
                    _logger.LogWarning("No OverlappingTrainingDateRequests found for Cohort Id {cohortId}", message.CohortId);
                    return;
                }

                if (overlappingTrainingDateRequest.Status == OverlappingTrainingDateRequestStatus.Pending)
                {
                    await _resolveOverlappingTrainingDateRequestService.Resolve(
                      overlappingTrainingDateRequest.PreviousApprenticeshipId,
                      null,
                      OverlappingTrainingDateRequestResolutionType.CohortDeleted);
                    return;
                }
                else
                {
                    _logger.LogWarning("Unable to modify OverlappingTrainingDateRequest {id} - status is already {status}", overlappingTrainingDateRequest.Id, overlappingTrainingDateRequest.Status);
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing CohortWithPendingOverlappingTrainingDateRequestDeletedEvent");
                throw;
            }
        }
    }
}