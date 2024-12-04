using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Services;

public class AcademicYearEndExpiryProcessorService(
    ILogger<AcademicYearEndExpiryProcessorService> logger,
    IAcademicYearDateProvider academicYearProvider,
    ProviderCommitmentsDbContext dbContext,
    ICurrentDateTime currentDateTime,
    IEventPublisher eventPublisher)
    : IAcademicYearEndExpiryProcessorService
{
    public async Task ExpireDataLocks(string jobId)
    {
        logger.LogInformation("{TypeName} run at {UtcNow} for Academic Year CurrentAcademicYearStartDate: {CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {LastAcademicYearFundingPeriod}, JobId: {JobId}",
            nameof(AcademicYearEndExpiryProcessorService),
            currentDateTime.UtcNow,
            academicYearProvider.CurrentAcademicYearStartDate,
            academicYearProvider.CurrentAcademicYearEndDate,
            academicYearProvider.LastAcademicYearFundingPeriod,
            jobId);

        var expirableDataLocks = await GetExpirableDataLocks(academicYearProvider.CurrentAcademicYearStartDate);
        long expiredCount = 0;

        foreach (var expirableDatalock in expirableDataLocks)
        {
            logger.LogInformation("Updating DataLockStatus for apprenticeshipId: {ApprenticeshipId} and PriceEpisodeIdentifier: {ApprenticeshipId}, JobId: {JobId}", expirableDatalock.ApprenticeshipId, expirableDatalock.ApprenticeshipId, jobId);

            expirableDatalock.IsExpired = true;
            expirableDatalock.Expired = currentDateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            expiredCount++;
        }

        logger.LogInformation("{TypeName} expired {ExpiredCount} items, JobId: {JobId}", nameof(AcademicYearEndExpiryProcessorService), expiredCount, jobId);
    }

    public async Task ExpireApprenticeshipUpdates(string jobId)
    {
        logger.LogInformation("{TypeName} run at {UtcNow} for Academic Year CurrentAcademicYearStartDate: {CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {LastAcademicYearFundingPeriod}, JobId: {JobId}",
            nameof(AcademicYearEndExpiryProcessorService),
            currentDateTime.UtcNow,
            academicYearProvider.CurrentAcademicYearStartDate,
            academicYearProvider.CurrentAcademicYearEndDate,
            academicYearProvider.LastAcademicYearFundingPeriod,
            jobId);

        var expiredApprenticeshipUpdatesQuery = GetExpirableApprenticeshipUpdates(academicYearProvider.CurrentAcademicYearStartDate);
        var expiredApprenticeshipUpdates = expiredApprenticeshipUpdatesQuery.ToList();

        logger.LogInformation("Found {Count} apprenticeship updates that will be set to expired, JobId: {JobId}", expiredApprenticeshipUpdates.Count, jobId);

        foreach (var apprenticeshipUpdate in expiredApprenticeshipUpdates)
        {
            logger.LogInformation("Updating ApprenticeshipUpdate to expired, ApprenticeshipUpdateId: {Id}, JobId: {JobId}", apprenticeshipUpdate.Id, jobId);

            var apprenticeship = await dbContext
                .Apprenticeships
                .Include(a => a.Cohort)
                .SingleOrDefaultAsync(app => app.Id == apprenticeshipUpdate.ApprenticeshipId);

            apprenticeshipUpdate.Status = ApprenticeshipUpdateStatus.Expired;
            apprenticeship.PendingUpdateOriginator = null;

            await dbContext.SaveChangesAsync();

            await eventPublisher.Publish(new ApprenticeshipUpdateCancelledEvent
            {
                AccountId = apprenticeship.Cohort.EmployerAccountId,
                ProviderId = apprenticeship.Cohort.ProviderId,
                ApprenticeshipId = apprenticeship.Id
            });
        }

        // re-enumerate same query variable
        if (expiredApprenticeshipUpdatesQuery.Count() != 0)
        {
            throw new Exception($"AcademicYearEndProcessor not completed successfully, Should not be any pending ApprenticeshipUpdates after job done, There are {expiredApprenticeshipUpdatesQuery.Count()} , JobId: {jobId}");
        }
    }

    private async Task<IEnumerable<DataLockStatus>> GetExpirableDataLocks(DateTime currentAcademicYearStartDate)
    {
        logger.LogInformation("Getting DataLocks to expire");

        return await dbContext.DataLocks
            .Where(app => app.IsExpired == false)
            .Where(app => app.IlrEffectiveFromDate < currentAcademicYearStartDate)
            .ToListAsync();
    }

    private IEnumerable<ApprenticeshipUpdate> GetExpirableApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
    {
        logger.LogInformation("Getting all expirable apprenticeship update");

        return dbContext.ApprenticeshipUpdates
            .Where(au => au.Status == ApprenticeshipUpdateStatus.Pending
                         && au.Apprenticeship.StartDate < currentAcademicYearStartDate
                         && (au.Cost != null || au.TrainingCode != null || au.StartDate != null));
    }
}