using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AcademicYearEndExpiryProcessorService : IAcademicYearEndExpiryProcessorService
    {
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AcademicYearEndExpiryProcessorService> _logger;
        private readonly ProviderCommitmentsDbContext _dbContext;

        public AcademicYearEndExpiryProcessorService(ILogger<AcademicYearEndExpiryProcessorService> logger,
            IAcademicYearDateProvider academicYearProvider,
            ProviderCommitmentsDbContext dbContext,
            ICurrentDateTime currentDateTime,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _currentDateTime = currentDateTime;
            _eventPublisher = eventPublisher;
            _academicYearProvider = academicYearProvider;
            _dbContext = dbContext;
        }

        public async Task ExpireDataLocks(string jobId)
        {
            _logger.LogInformation($"{nameof(AcademicYearEndExpiryProcessorService)} run at {_currentDateTime.UtcNow} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");

            await _dbContext.Database.ExecuteSqlRawAsync(
                "update [DataLockStatus] set IsExpired=1, Expired=GETDATE() where IsExpired=0 AND IlrEffectiveFromDate < @CurrentAcademicYearStartDate",
                new SqlParameter("CurrentAcademicYearStartDate",
                    _academicYearProvider.CurrentAcademicYearStartDate));

            await _dbContext.SaveChangesAsync();
        }

        public async Task ExpireApprenticeshipUpdates(string jobId)
        {
            _logger.LogInformation($"{nameof(AcademicYearEndExpiryProcessorService)} run at {_currentDateTime.UtcNow} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");

            var expiredApprenticeshipUpdatesQuery = GetExpirableApprenticeshipUpdates(_academicYearProvider.CurrentAcademicYearStartDate);
            var expiredApprenticeshipUpdates = expiredApprenticeshipUpdatesQuery.ToList();

            _logger.LogInformation($"Found {expiredApprenticeshipUpdates.Count} apprenticeship updates that will be set to expired, JobId: {jobId}");

            foreach (var apprenticeshipUpdate in expiredApprenticeshipUpdates)
            {
                _logger.LogInformation($"Updating ApprenticeshipUpdate to expired, ApprenticeshipUpdateId: {apprenticeshipUpdate.Id}, JobId: {jobId}");

                var apprenticeship = await _dbContext
                    .Apprenticeships
                    .Include(a => a.Cohort)
                    .SingleOrDefaultAsync(app => app.Id == apprenticeshipUpdate.ApprenticeshipId);

                apprenticeshipUpdate.Status = ApprenticeshipUpdateStatus.Expired;
                apprenticeship.PendingUpdateOriginator = null;

                await _dbContext.SaveChangesAsync();

                await _eventPublisher.Publish(new ApprenticeshipUpdateCancelledEvent
                {
                    AccountId = apprenticeship.Cohort.EmployerAccountId,
                    ProviderId = apprenticeship.Cohort.ProviderId,
                    ApprenticeshipId = apprenticeship.Id
                });
            }

            // re-enumerate same query variable
            if (expiredApprenticeshipUpdatesQuery.Count() != 0)
            {
                throw new Exception($"AcademicYearEndProcessor not completed successfull, Should not be any pending ApprenticeshipUpdates after job done, There are {expiredApprenticeshipUpdatesQuery.Count()} , JobId: {jobId}");
            }
        }

        private IEnumerable<ApprenticeshipUpdate> GetExpirableApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
        {
            _logger.LogInformation("Getting all expirable apprenticeship update");

            return _dbContext.ApprenticeshipUpdates
                .Where(au => au.Status == ApprenticeshipUpdateStatus.Pending
                    && au.Apprenticeship.StartDate < currentAcademicYearStartDate
                    && (au.Cost != null || au.TrainingCode != null || au.StartDate != null));
        }
    }
}