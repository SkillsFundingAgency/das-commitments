using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop
{
    public class GetPendingOverlappingTrainingDatesToStopHandler : IRequestHandler<GetPendingOverlappingTrainingDatesToStopQuery, GetPendingOverlappingTrainingDatesToStopResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<GetPendingOverlappingTrainingDatesToStopHandler> _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public GetPendingOverlappingTrainingDatesToStopHandler(Lazy<ProviderCommitmentsDbContext> db,
            ICurrentDateTime currentDateTime,
            ILogger<GetPendingOverlappingTrainingDatesToStopHandler> logger)
        {
            _dbContext = db;
            _currentDateTime = currentDateTime;
            _logger = logger;
        }

        public async Task<GetPendingOverlappingTrainingDatesToStopResult> Handle(GetPendingOverlappingTrainingDatesToStopQuery request, CancellationToken cancellationToken)
        {
            var currentDate = _currentDateTime.UtcNow;

            var pendingRecords = await _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => 
                        ((x.PreviousApprenticeship.PaymentStatus == PaymentStatus.Active && x.PreviousApprenticeship.StartDate < currentDate) ||
                        x.PreviousApprenticeship.PaymentStatus == PaymentStatus.Paused) &&
                        x.NotifiedServiceDeskOn == null
                        && x.Status == OverlappingTrainingDateRequestStatus.Pending
                        && x.CreatedOn < currentDate.AddDays(-14).Date)
                .ToListAsync();

            _logger.LogInformation("Found {Count} records which can be auto-stopped after 2 weeks for overlapping training dates.", pendingRecords.Count);

            return new GetPendingOverlappingTrainingDatesToStopResult
            {
                OverlappingTrainingDateRequests = pendingRecords
            };
        }
    }
}
