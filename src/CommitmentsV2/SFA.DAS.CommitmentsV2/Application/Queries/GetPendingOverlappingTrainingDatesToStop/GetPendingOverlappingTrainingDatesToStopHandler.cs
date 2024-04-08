using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop
{
    public class GetPendingOverlappingTrainingDatesToStopHandler : IRequestHandler<GetPendingOverlappingTrainingDatesToStopQuery, GetPendingOverlappingTrainingDatesToStopResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<GetPendingOverlappingTrainingDatesToStopHandler> _logger;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly CommitmentsV2Configuration _configuration;

        public GetPendingOverlappingTrainingDatesToStopHandler(Lazy<ProviderCommitmentsDbContext> db,
            ICurrentDateTime currentDateTime,
                        CommitmentsV2Configuration configuration,

            ILogger<GetPendingOverlappingTrainingDatesToStopHandler> logger)
        {
            _dbContext = db;
            _currentDateTime = currentDateTime;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GetPendingOverlappingTrainingDatesToStopResult> Handle(GetPendingOverlappingTrainingDatesToStopQuery request, CancellationToken cancellationToken)
        {
            if (_configuration.OLTD_GoLiveDate.HasValue)
            {
                _logger.LogInformation("OLTD_GoLiveDate {value}", _configuration.OLTD_GoLiveDate.Value);
            }
            else
            {
                _logger.LogInformation("OLTD_GoLiveDate has no value");
            }

            var currentDate = _currentDateTime.UtcNow;
            var goLiveDate = _configuration.OLTD_GoLiveDate ?? DateTime.MinValue;

            var pendingRecords = await _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && (x.CreatedOn < goLiveDate ? x.CreatedOn < currentDate.AddDays(-28).Date
                            : x.CreatedOn < currentDate.AddDays(-14).Date))
                .ToListAsync();

            _logger.LogInformation("Found {Count} records which can be auto-stopped after 2 weeks for overlapping training dates.", pendingRecords.Count);

            return new GetPendingOverlappingTrainingDatesToStopResult
            {
                OverlappingTrainingDateRequests = pendingRecords
            };
        }
    }
}
