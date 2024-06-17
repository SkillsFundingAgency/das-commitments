using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler : IRequestHandler<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand>
    {
        public const string TemplateId = "ExpiredOverlappingTrainingDateForServiceDesk";
        private readonly ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler> _logger;
        private readonly IMessageSession _messageSession;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly CommitmentsV2Configuration _configuration;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler(Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,        
            IMessageSession messageSession,
            ICurrentDateTime currentDateTime,
            CommitmentsV2Configuration configuration,
            ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler> logger)
        {
            _dbContext = commitmentsDbContext;
            _currentDateTime = currentDateTime;
            _messageSession = messageSession;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Handle(OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var currentDate = _currentDateTime.UtcNow;

                var pendingRecords = await _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && x.CreatedOn < currentDate.AddDays(-14).Date)
                .ToListAsync(cancellationToken);

                if (pendingRecords != null && pendingRecords.Any())
                {
                    int zenDeskCount = 0;
                    int stoppingCount = 0;

                    foreach (var request in pendingRecords)
                    {
                        if (request.DraftApprenticeship != null)
                        {
                            switch (request.PreviousApprenticeship.PaymentStatus)
                            {
                                case PaymentStatus.Withdrawn:
                                case PaymentStatus.Completed:
                                    zenDeskCount++;
                                    await SendToZenDesk(request);
                                    break;
                                case PaymentStatus.Active:
                                case PaymentStatus.Paused:
                                    stoppingCount++;
                                    await AutoStopOverlappingTrainingDateRequest(request);
                                    break;
                                default:
                                    _logger.LogWarning("Unhandled PaymentStatus for OverlappingTrainingDateRequest ID: {requestId}, PaymentStatus: {paymentStatus}", request.Id, request.PreviousApprenticeship.PaymentStatus);
                                    break;
                            }
                        }
                    }

                    _logger.LogInformation("Found {count} OLTDs to send to ZenDesk", zenDeskCount);
                    _logger.LogInformation("Found {count} OLTDs to send to automatically stop", stoppingCount);

                    await _dbContext.Value.SaveChangesAsync(default);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while automatically stopping overlapping training date requests");

                throw;
            }
        }

        private async Task AutoStopOverlappingTrainingDateRequest(OverlappingTrainingDateRequest request)
        {
            _logger.LogInformation("Sending StopApprenticeshipRequest for ApprenticeshipId {PreviousApprenticeshipId}", request.PreviousApprenticeshipId);

            await _messageSession.Send(new AutomaticallyStopOverlappingTrainingDateRequestCommand(
                request.PreviousApprenticeship.Cohort.EmployerAccountId,
                request.PreviousApprenticeshipId,
                request.DraftApprenticeship.StartDate.Value,
                false,
                UserInfo.System,
                Party.Employer));
        }

        private async Task SendToZenDesk(OverlappingTrainingDateRequest request)
        {
            var tokens = new Dictionary<string, string>
            {
                { "RequestCreatedByProviderEmail", string.IsNullOrWhiteSpace(request.RequestCreatedByProviderEmail) ? "Not available" : request.RequestCreatedByProviderEmail },
                { "LastUpdatedByProviderEmail", request.DraftApprenticeship?.Cohort?.LastUpdatedByProviderEmail },
                { "ULN", request.DraftApprenticeship?.Uln },
                { "NewProviderUkprn", request.DraftApprenticeship?.Cohort?.ProviderId.ToString() },
                { "OldProviderUkprn", request.PreviousApprenticeship?.Cohort?.ProviderId.ToString() }
            };

            var emailCommand = new SendEmailCommand(TemplateId, _configuration.ZenDeskEmailAddress, tokens);
            await _messageSession.Send(emailCommand);
            request.NotifiedServiceDeskOn = _currentDateTime.UtcNow;
        }
    }
}