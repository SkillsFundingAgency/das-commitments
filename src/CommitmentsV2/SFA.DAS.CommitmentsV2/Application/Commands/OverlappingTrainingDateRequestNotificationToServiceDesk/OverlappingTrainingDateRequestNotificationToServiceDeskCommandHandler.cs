using System;
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
        public const string TemplateId = "OverlappingTraingDateRequestNotificationForServiceDesk";
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
        public async Task<Unit> Handle(OverlappingTrainingDateRequestNotificationToServiceDeskCommand request, CancellationToken cancellationToken)
        {
            var dateTime = _currentDateTime.UtcNow.AddDays(-28).Date;

            var sendEmailToServiceDeskForRecords = _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeshp => draftApprenticeshp.Cohort)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && x.CreatedOn < dateTime)
                .ToList();

            _logger.LogInformation($"Found {sendEmailToServiceDeskForRecords.Count} records which need overlapping training reminder for Service Desk");


            foreach (var x in sendEmailToServiceDeskForRecords)
            {
                var showDOB = x.DraftApprenticeship.DateOfBirth.HasValue ? "Yes" : "No";
                var tokens = new Dictionary<string, string>
                {
                    { "RequestCreatedByProviderEmail", x.RequestCreatedByProviderEmail },
                    { "ULN", x.DraftApprenticeship.Uln },
                    { "NewProviderUkprn", x.DraftApprenticeship.Cohort.ProviderId.ToString() },
                    { "OldProviderUkprn", x.PreviousApprenticeship.Cohort.ProviderId.ToString() }
                };

                var emailCommand = new SendEmailCommand(TemplateId,_configuration.ZenDeskEmailAddress, tokens);
                await _messageSession.Send(emailCommand);

                x.NotifiedServiceDeskOn = _currentDateTime.UtcNow;
            }

            await _dbContext.Value.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
