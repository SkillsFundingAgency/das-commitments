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
            _logger.LogInformation($"CohortDeletedEvent received for Cohort {message.CohortId}, with pending OverlappingTrainingDateRequest");

            try
            {
                var cohort = await _dbContext.Value.Cohorts
                    .Include(x => x.Apprenticeships)
                    .Where(x => x.Id == message.CohortId)
                    .FirstOrDefaultAsync();

                if (cohort == null)
                {
                    _logger.LogWarning($"No cohort found for Id {message.CohortId}");
                    return;
                }

                var overlappingTrainingDateRequest = cohort.Apprenticeships.FirstOrDefault().OverlappingTrainingDateRequests?.OrderByDescending(x => x.Id).FirstOrDefault();

                if (overlappingTrainingDateRequest == null)
                {
                    _logger.LogWarning($"No OverlappingTrainingDateRequests found for Cohort Id {message.CohortId}");
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
                    _logger.LogWarning($"Unable to modify OverlappingTrainingDateRequest {overlappingTrainingDateRequest.Id} - status is already {overlappingTrainingDateRequest.Status}");
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing CohortWithPendingOverlappingTrainingDateRequestDeletedEvent", e);
                throw;
            }
        }
    }
}