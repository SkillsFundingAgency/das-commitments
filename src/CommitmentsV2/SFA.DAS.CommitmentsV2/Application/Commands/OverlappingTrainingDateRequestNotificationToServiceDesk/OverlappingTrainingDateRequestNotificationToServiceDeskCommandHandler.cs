﻿using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk
{
    public class OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler : IRequestHandler<OverlappingTrainingDateRequestNotificationToServiceDeskCommand>
    {
        public const string TemplateId = "ExpiredOverlappingTrainingDateForServiceDesk";
        private readonly ICurrentDateTime _currentDateTime;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IMessageSession _messageSession;
        private readonly CommitmentsV2Configuration _configuration;
        private readonly ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> _logger;

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

            await _dbContext.Value.SaveChangesAsync(cancellationToken);
        }    
    }
}
