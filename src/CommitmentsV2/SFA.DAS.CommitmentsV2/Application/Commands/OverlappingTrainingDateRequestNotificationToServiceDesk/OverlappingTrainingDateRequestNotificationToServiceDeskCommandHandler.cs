using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk
{
    public class OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler : IRequestHandler<OverlappingTrainingDateRequestNotificationToServiceDeskCommand>
    {
        private ICurrentDateTime _currentDateTime;
        private Lazy<ProviderCommitmentsDbContext> _dbContext;
        private ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> _logger;

        public OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler(Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
            ICurrentDateTime currentDateTime,
            ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler> logger)
        {
            _currentDateTime = currentDateTime;
            _dbContext = commitmentsDbContext;
            _logger = logger;
        }
        public Task<Unit> Handle(OverlappingTrainingDateRequestNotificationToServiceDeskCommand request, CancellationToken cancellationToken)
        {
            var dateTime = _currentDateTime.UtcNow.AddDays(-28).Date;

            var sendEmailToServiceDeskForRecords = _dbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeshp => draftApprenticeshp.Cohort)
                        .ThenInclude(cohort => cohort.Provider)
                .Include(oltd => oltd.DraftApprenticeship)
                    .ThenInclude(draftApprenticeshp => draftApprenticeshp.Cohort)
                        .ThenInclude(cohort => cohort.AccountLegalEntity)
               .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                        .ThenInclude(cohort => cohort.Provider)
                .Include(oltd => oltd.PreviousApprenticeship)
                    .ThenInclude(draftApprenticeshp => draftApprenticeshp.Cohort)
                        .ThenInclude(cohort => cohort.AccountLegalEntity)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            && x.CreatedOn < dateTime)
                .Select(x => new
                            {
                                Id = x.Id,
                                ApprenticeName = x.DraftApprenticeship.FirstName + " " + x.DraftApprenticeship.LastName,,
                                ULN = x.DraftApprenticeship.Uln,
                                NewProviderName = x.DraftApprenticeship.Cohort.Provider.Name,
                                NewProviderUkprn = x.DraftApprenticeship.Cohort.ProviderId,
                                NewEmployerName = x.DraftApprenticeship.Cohort.AccountLegalEntity.Name,
                                DOB = x.DraftApprenticeship.DateOfBirth,
                                CourseName = x.DraftApprenticeship.CourseName,
                                StartDate = x.DraftApprenticeship.StartDate,
                                OldProviderName = x.PreviousApprenticeship.Cohort.Provider.Name,
                                OldProviderUkprn = x.PreviousApprenticeship.Cohort.ProviderId,
                                OldEmployerName = x.PreviousApprenticeship.Cohort.AccountLegalEntity.Name
                            }).ToList();

            _logger.LogInformation($"Found {sendEmailToServiceDeskForRecords.Count} records which need overlapping training reminder for Service Desk");


            foreach (var record in sendEmailToServiceDeskForRecords)
            {
                var tokens = new Dictionary<string, string>();
                tokens.Add("ApprenticeName", record.ApprenticeName);
                tokens.Add("DOB", record.DOB?.ToString("dd-MM-YYYY") ?? "");
                tokens.Add("CourseName", record.CourseName);
                tokens.Add("StartDate", record.StartDate?.ToString("dd-MM-YYYY") ?? "");
                tokens.Add("NewEmployerName", record.NewEmployerName);
                tokens.Add("NewProviderName", record.NewProviderName);
                tokens.Add("NewProviderUkprn", record.NewProviderUkprn.ToString());
                tokens.Add("OldEmployerName", record.OldEmployerName);
                tokens.Add("OldProviderName", record.OldProviderName);
                tokens.Add("OldProviderUkprn", record.OldProviderUkprn.ToString());
                
                var emailCommand = new SendEmailCommand("OverlappingTraingDateRequestNotificationForServiceDesk","emailAddress", tokens);


            }

            return Task.FromResult(Unit.Value);
        }
    }
}
