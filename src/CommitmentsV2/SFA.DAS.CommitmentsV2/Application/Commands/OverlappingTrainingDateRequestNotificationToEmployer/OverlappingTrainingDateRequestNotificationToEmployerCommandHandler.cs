using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer
{
    internal class OverlappingTrainingDateRequestNotificationToEmployerCommandHandler : IRequestHandler<OverlappingTrainingDateRequestNotificationToEmployerCommand>
    {
        public const string TemplateId = "ExpiredOverlappingTrainingDateForServiceDesk";
        private ICurrentDateTime _currentDateTime;
        private Lazy<ProviderCommitmentsDbContext> _dbContext;
        private IMessageSession _messageSession;
        private readonly CommitmentsV2Configuration _configuration;
        private ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> _logger;

        public OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler(Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
            ICurrentDateTime currentDateTime,
            IMessageSession messageSession,
            CommitmentsV2Configuration configuration,
            ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> logger)
        {
            _dbContext = commitmentsDbContext;
            _currentDateTime = currentDateTime;
            _messageSession = messageSession;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<Unit> Handle(OverlappingTrainingDateRequestNotificationToEmployerCommand request, CancellationToken cancellationToken)
        {
            var dateTime = _currentDateTime.UtcNow.AddDays(-28).Date;

            var pendingRecords = _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && x.CreatedOn < dateTime
                            )
                .ToList();

            _logger.LogInformation($"Found {pendingRecords.Count} records which need overlapping training reminder for Service Desk");

            hkf - tech marked this conversation as resolved.
            foreach (var pendingRecord in pendingRecords)
            {
                if (pendingRecord.DraftApprenticeship != null)
                {
                    var tokens = new Dictionary<string, string>
                    {
                        { "RequestCreatedByProviderEmail", string.IsNullOrWhiteSpace(pendingRecord.RequestCreatedByProviderEmail) ? "Not available" : pendingRecord.RequestCreatedByProviderEmail },
                        { "LastUpdatedByProviderEmail", pendingRecord.DraftApprenticeship?.Cohort?.LastUpdatedByProviderEmail },
                        { "ULN", pendingRecord.DraftApprenticeship?.Uln },
                        { "NewProviderUkprn", pendingRecord.DraftApprenticeship?.Cohort?.ProviderId.ToString() },
                        { "OldProviderUkprn", pendingRecord.PreviousApprenticeship?.Cohort?.ProviderId.ToString() }
                    };

                    var emailCommand = new SendEmailCommand(TemplateId, _configuration.ZenDeskEmailAddress, tokens);
                    await _messageSession.Send(emailCommand);

                    pendingRecord.NotifiedServiceDeskOn = _currentDateTime.UtcNow;
                }
            }

            await _dbContext.Value.SaveChangesAsync();
            return Unit.Value;
        }
    }
