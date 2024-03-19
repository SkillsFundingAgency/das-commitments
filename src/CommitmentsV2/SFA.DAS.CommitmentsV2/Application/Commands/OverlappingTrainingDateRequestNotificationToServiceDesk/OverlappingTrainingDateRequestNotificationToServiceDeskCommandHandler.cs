using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk
{
    public class OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler : IRequestHandler<OverlappingTrainingDateRequestNotificationToServiceDeskCommand>
    {
        public const string TemplateId = "ExpiredOverlappingTrainingDateForServiceDesk";
        private readonly ICurrentDateTime _currentDateTime;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IApprovalsOuterApiClient _apiClient;
        private readonly CommitmentsV2Configuration _configuration;
        private readonly ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> _logger;

        public OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler(Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
            ICurrentDateTime currentDateTime,
            CommitmentsV2Configuration configuration,
            IApprovalsOuterApiClient apiClient,
            ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> logger)
        {
            _dbContext = commitmentsDbContext;
            _currentDateTime = currentDateTime;
            _configuration = configuration;
            _apiClient = apiClient;
            _logger = logger;
        }
        public async Task Handle(OverlappingTrainingDateRequestNotificationToServiceDeskCommand request, CancellationToken cancellationToken)
        {
            if (_configuration.OLTD_GoLiveDate.HasValue)
            {
                _logger.LogInformation($"OLTD_GoLiveDate {_configuration.OLTD_GoLiveDate.Value.ToString()}");
            }
            else
            {
                _logger.LogInformation($"OLTD_GoLiveDate has no value");
            }

            var currentDate = _currentDateTime.UtcNow;
            var goLiveDate = _configuration.OLTD_GoLiveDate ?? DateTime.MinValue;

            var pendingRecords = _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && (x.CreatedOn < goLiveDate ? x.CreatedOn < currentDate.AddDays(-28).Date
                            : x.CreatedOn < currentDate.AddDays(-14).Date))
                .ToList();

            _logger.LogInformation($"Found {pendingRecords.Count} records which need overlapping training reminder for Service Desk");

            foreach (var pendingRecord in pendingRecords)
            {
                if (pendingRecord.DraftApprenticeship != null)
                {
                    var body = new StopApprenticeshipRequestRequest.Body
                    {
                        AccountId = pendingRecord.PreviousApprenticeship.Cohort.EmployerAccountId,
                        MadeRedundant = false,
                        StopDate = pendingRecord.DraftApprenticeship.StartDate.Value,
                        UserInfo = Types.UserInfo.System
                    };

                    var stopRequest = new StopApprenticeshipRequestRequest(
                        pendingRecord.PreviousApprenticeshipId,
                        body
                        );

                    _logger.LogInformation($"Sending StopApprenticeshipRequest for ApprenticeshipId {pendingRecord.PreviousApprenticeshipId}");

                    await _apiClient.PostAsync<StopApprenticeshipRequestRequest>(stopRequest);
                }
            }
        }
    }
}
