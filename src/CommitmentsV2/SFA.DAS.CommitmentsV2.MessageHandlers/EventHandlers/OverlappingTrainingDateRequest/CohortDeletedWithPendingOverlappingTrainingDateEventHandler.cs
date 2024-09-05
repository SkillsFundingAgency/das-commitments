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
                    .IgnoreQueryFilters()
                    .Include(x => x.Apprenticeships)
                    .ThenInclude(a => a.OverlappingTrainingDateRequests)
                    .Where(x => x.Id == message.CohortId)
                    .FirstOrDefaultAsync();

                if (cohort == null)
                {
                    _logger.LogWarning("No cohort found for Id {cohortId}", message.CohortId);
                    return;
                }

                var overlappingTrainingDateRequests = cohort.Apprenticeships
                  .Where(apprenticeship => apprenticeship.OverlappingTrainingDateRequests != null)
                  .OrderByDescending(apprenticeship => apprenticeship.CreatedOn)
                  .SelectMany(apprenticeship => apprenticeship.OverlappingTrainingDateRequests)
                  .OrderByDescending(request => request.Id)
                  .ToList();

                if (!overlappingTrainingDateRequests.Any())
                {
                    _logger.LogWarning("No OverlappingTrainingDateRequests found for Cohort Id {cohortId}", message.CohortId);
                    return;
                }

                foreach (var request in overlappingTrainingDateRequests)
                {
                    if (request.Status == OverlappingTrainingDateRequestStatus.Pending)
                    {
                        _logger.LogInformation("Resolving OverlappingTrainingDateRequest {id} for Cohort Id {cohortId}", request.Id, message.CohortId);

                        await _resolveOverlappingTrainingDateRequestService.Resolve(
                          request.PreviousApprenticeshipId,
                          null,
                          OverlappingTrainingDateRequestResolutionType.CohortDeleted);
                    }
                    else
                    {
                        _logger.LogWarning("Unable to modify OverlappingTrainingDateRequest {id} - status is already {status}", request.Id, request.Status);
                    }
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