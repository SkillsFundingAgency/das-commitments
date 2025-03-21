﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System.Diagnostics;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Globalization;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Entities.DataLockProcessing;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class DataLockUpdaterService : IDataLockUpdaterService
{
    private readonly ILogger<DataLockUpdaterService> _logger;
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly IApprovalsOuterApiClient _outerApiClient;
    private readonly CommitmentPaymentsWebJobConfiguration _config;
    private readonly IFilterOutAcademicYearRollOverDataLocks _filterOutAcademicYearRollOverDataLocks;
    private readonly List<DataLockErrorCode> _whiteList;

    private readonly DateTime _1718AcademicYearStartDate = new(2017, 08, 01);

    public DataLockUpdaterService(ILogger<DataLockUpdaterService> logger,
        Lazy<ProviderCommitmentsDbContext> db,
        IApprovalsOuterApiClient outerApiClient,
        CommitmentPaymentsWebJobConfiguration config,
        IFilterOutAcademicYearRollOverDataLocks filterOutAcademicYearRollOverDataLocks)
    {
        _logger = logger;
        _db = db;
        _outerApiClient = outerApiClient;
        _config = config;
        _filterOutAcademicYearRollOverDataLocks = filterOutAcademicYearRollOverDataLocks;
        _whiteList =
        [
            DataLockErrorCode.Dlock03,
            DataLockErrorCode.Dlock04,
            DataLockErrorCode.Dlock05,
            DataLockErrorCode.Dlock06,
            DataLockErrorCode.Dlock07
        ];
    }

    public async Task RunUpdate()
    {
        var history = new DataLockUpdaterJobHistory();

        _logger.LogInformation("Retrieving last DataLock Event Id from repository");
        var lastId = await GetLastDataLockEventId();

        _logger.LogInformation("Retrieving page of data from Payment Events Service since Event Id {LastId}", lastId);
        
        var stopwatch = Stopwatch.StartNew();

        var datalockStatusResponse = await _outerApiClient.GetWithRetry<GetDataLockStatusListResponse>(new GetDataLockEventsRequest(lastId));

        var page = datalockStatusResponse?.DataLockStatuses ?? new List<DataLockStatus>();

        stopwatch.Stop();
        
        _logger.LogInformation("Response took {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

        history.FromEventId = lastId;
        history.ItemCount = page.Count;
        history.PagesRemaining = page.Count != 0 ? datalockStatusResponse.TotalNumberOfPages - 1 : 0;

        if (page.Count == 0)
        {
            _logger.LogInformation("No data returned; exiting");
            await StoreJobHistory(history);
            return;
        }

        _logger.LogInformation("{PageCount} records returned in page", page.Count);

        foreach (var dataLockStatus in page)
        {
            _logger.LogInformation("Read datalock Apprenticeship {ApprenticeshipId} Event Id {DataLockEventId} Status {ErrorCode} and EventStatus: {EventStatus}", dataLockStatus.ApprenticeshipId, dataLockStatus.DataLockEventId, dataLockStatus.ErrorCode, dataLockStatus.EventStatus);

            var datalockSuccess = dataLockStatus.ErrorCode == DataLockErrorCode.None;

            if (!datalockSuccess)
            {
                ApplyErrorCodeWhiteList(dataLockStatus);
            }

            var is1617 = GetDateFromPriceEpisodeIdentifier(dataLockStatus) < _1718AcademicYearStartDate;
            if (is1617)
            {
                _logger.LogInformation("Data lock Event Id {DataLockEventId} pertains to 16/17 academic year and will be ignored", dataLockStatus.DataLockEventId);
            }

            if ((datalockSuccess || dataLockStatus.ErrorCode != DataLockErrorCode.None) && !is1617)
            {
                var apprenticeship = await GetApprenticeship(dataLockStatus.ApprenticeshipId);

                //temporarily ignore dlock7 & 9 combos until payments R14 fixes properly
                if (dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07) && dataLockStatus.IlrEffectiveFromDate < apprenticeship.StartDate)
                {
                    _logger.LogInformation("Ignoring datalock for Apprenticeship #{ApprenticeshipId} Dlock07 with Effective Date before Start Date. Event Id {DataLockEventId}",
                        dataLockStatus.ApprenticeshipId, dataLockStatus.DataLockEventId);
                }
                else
                {
                    _logger.LogInformation("Updating Apprenticeship {ApprenticeshipId} Event Id {DataLockEventId} Status {ErrorCode}", dataLockStatus.ApprenticeshipId, dataLockStatus.DataLockEventId, dataLockStatus.ErrorCode);

                    AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(apprenticeship, dataLockStatus);

                    try
                    {
                        var result = await UpdateDataLockStatus(dataLockStatus);

                        if (result.IsExpired)
                        {
                            _logger.LogInformation("Datalock for Apprenticeship {ApprenticeshipId}, PriceEpisodeIdentifier {PriceEpisodeIdentifier}, Event Id {DataLockEventId} identified as having expired",
                                dataLockStatus.ApprenticeshipId,
                                dataLockStatus.PriceEpisodeIdentifier,
                                dataLockStatus.DataLockEventId);
                            
                            history.ExpiredCount++;
                        }
                        if(result.IsDuplicate)
                        {
                            _logger.LogInformation("Datalock for Apprenticeship {ApprenticeshipId}, PriceEpisodeIdentifier {PriceEpisodeIdentifier}, Event Id {DataLockEventId} identified as being a duplicate",
                                dataLockStatus.ApprenticeshipId,
                                dataLockStatus.PriceEpisodeIdentifier,
                                dataLockStatus.DataLockEventId);
                            
                            history.DuplicateCount++;
                        }

                        await _filterOutAcademicYearRollOverDataLocks.Filter(dataLockStatus.ApprenticeshipId);
                    }
                    catch (RepositoryConstraintException ex) when (_config.IgnoreDataLockStatusConstraintErrors)
                    {
                        _logger.LogWarning(ex, "Exception in DataLock updater");
                    }

                    if (datalockSuccess)
                    {
                        await SetHasHadDataLockSuccess(dataLockStatus.ApprenticeshipId);

                        var pendingUpdate = await GetPendingApprenticeshipUpdate(dataLockStatus.ApprenticeshipId);

                        if (pendingUpdate != null && (pendingUpdate.Cost != null || pendingUpdate.TrainingCode != null))
                        {
                            await ExpireApprenticeshipUpdate(pendingUpdate.Id);
                            _logger.LogInformation("Pending ApprenticeshipUpdate {Id} expired due to successful data lock event {DataLockEventId}", pendingUpdate.Id, dataLockStatus.DataLockEventId);
                        }
                    }
                }
            }

            lastId = dataLockStatus.DataLockEventId;
        }

        await StoreLastDataLockEventId(lastId);
        await StoreJobHistory(history);
    }

    private void AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(Apprenticeship apprenticeship, DataLockStatus datalock)
    {
        if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn &&
            apprenticeship.StopDate == apprenticeship.StartDate)
        {
            _logger.LogInformation("Auto-resolving datalock for Apprenticeship #{ApprenticeshipId} withdrawn effective at start date. Event Id {DataLockEventId}", datalock.ApprenticeshipId, datalock.DataLockEventId);

            datalock.IsResolved = true;
        }
    }

    private void ApplyErrorCodeWhiteList(DataLockStatus dataLockStatus)
    {
        var whitelisted = DataLockErrorCode.None;
        var skipped = DataLockErrorCode.None;

        foreach (DataLockErrorCode errorCode in Enum.GetValues(typeof(DataLockErrorCode)))
        {
            if (dataLockStatus.ErrorCode.HasFlag(errorCode))
            {
                if (_whiteList.Contains(errorCode))
                {
                    whitelisted = whitelisted == DataLockErrorCode.None ? errorCode : whitelisted | errorCode;
                }
                else
                {
                    skipped = skipped == DataLockErrorCode.None ? errorCode : skipped | errorCode;
                }
            }
        }

        if (skipped != DataLockErrorCode.None)
        {
            _logger.LogInformation("Skipping {Skipped}", skipped);
        }

        dataLockStatus.ErrorCode = whitelisted;
    }

    private async Task<long> GetLastDataLockEventId()
    {
        var lastEvent = await _db.Value.DataLockUpdaterJobStatuses.SingleOrDefaultAsync();
        return lastEvent?.LastEventId ?? await _db.Value.DataLocks.MaxAsync(x => x.DataLockEventId);
    }

    private async Task StoreLastDataLockEventId(long lastId)
    {
        var lastEvent = await _db.Value.DataLockUpdaterJobStatuses.SingleOrDefaultAsync();

        if (lastEvent == null)
        {
            lastEvent = new DataLockUpdaterJobStatus
            {
                LastEventId = lastId
            };

            await _db.Value.DataLockUpdaterJobStatuses.AddAsync(lastEvent);
        }
        else
        {
            lastEvent.LastEventId = lastId;
        }

        await _db.Value.SaveChangesAsync();
    }

    private async Task StoreJobHistory(DataLockUpdaterJobHistory history)
    {
        history.FinishedOn = DateTime.UtcNow;
        await _db.Value.DataLockUpdaterJobHistory.AddAsync(history);
        await _db.Value.SaveChangesAsync();
    }

    private static DateTime GetDateFromPriceEpisodeIdentifier(DataLockStatus dataLockStatus)
    {
        return DateTime.ParseExact(dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy", new CultureInfo("en-GB"));
    }

    private async Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId)
    {
        var apprenticeshipUpdate = await _db.Value.ApprenticeshipUpdates
            .FirstOrDefaultAsync(x => x.ApprenticeshipId == apprenticeshipId &&
                                      x.Status == ApprenticeshipUpdateStatus.Pending);

        return apprenticeshipUpdate;
    }

    private async Task<Apprenticeship> GetApprenticeship(long apprenticeshipId)
    {
        var apprenticeship = await _db.Value.Apprenticeships
            .FirstOrDefaultAsync(x => x.Id == apprenticeshipId);
        return apprenticeship;
    }

    private async Task<DataLockUpdateResult> UpdateDataLockStatus(DataLockStatus dataLockStatus)
    {
        var result = new DataLockUpdateResult();

        var datalock = await _db.Value.DataLocks
            .FirstOrDefaultAsync(x => x.ApprenticeshipId == dataLockStatus.ApprenticeshipId
                                      && x.PriceEpisodeIdentifier.ToLower() == dataLockStatus.PriceEpisodeIdentifier.ToLower());

        if (datalock == null)
        {
            var newDataLockStatus = new DataLockStatus
            {
                ApprenticeshipId = dataLockStatus.ApprenticeshipId,
                DataLockEventId = dataLockStatus.DataLockEventId,
                DataLockEventDatetime = dataLockStatus.DataLockEventDatetime,
                PriceEpisodeIdentifier = dataLockStatus.PriceEpisodeIdentifier,
                IlrTrainingCourseCode = dataLockStatus.IlrTrainingCourseCode,
                IlrTrainingType = dataLockStatus.IlrTrainingType,
                IlrActualStartDate = dataLockStatus.IlrActualStartDate,
                IlrEffectiveFromDate = dataLockStatus.IlrEffectiveFromDate,
                IlrPriceEffectiveToDate = dataLockStatus.IlrPriceEffectiveToDate,
                IlrTotalCost = dataLockStatus.IlrTotalCost,
                ErrorCode = dataLockStatus.ErrorCode,
                Status = dataLockStatus.Status,
                TriageStatus = dataLockStatus.TriageStatus,
                ApprenticeshipUpdateId = dataLockStatus.ApprenticeshipUpdateId,
                IsResolved = dataLockStatus.IsResolved,
                EventStatus = dataLockStatus.EventStatus,
            };

            await _db.Value.DataLocks.AddAsync(newDataLockStatus);

        }
        else
        {
            if (datalock.IsExpired)
            {
                result.IsExpired = true;
            }

            if (datalock.IsDuplicate(dataLockStatus))
            {
                result.IsDuplicate = true;
            }

            datalock.ApprenticeshipId = dataLockStatus.ApprenticeshipId;
            datalock.DataLockEventId = dataLockStatus.DataLockEventId;
            datalock.DataLockEventDatetime = dataLockStatus.DataLockEventDatetime;
            datalock.PriceEpisodeIdentifier = dataLockStatus.PriceEpisodeIdentifier;
            datalock.IlrTrainingCourseCode = dataLockStatus.IlrTrainingCourseCode;
            datalock.IlrTrainingType = dataLockStatus.IlrTrainingType;
            datalock.IlrActualStartDate = dataLockStatus.IlrActualStartDate;
            datalock.IlrEffectiveFromDate = dataLockStatus.IlrEffectiveFromDate;
            datalock.IlrPriceEffectiveToDate = dataLockStatus.IlrPriceEffectiveToDate;
            datalock.IlrTotalCost = dataLockStatus.IlrTotalCost;
            datalock.ErrorCode = dataLockStatus.ErrorCode;
            datalock.Status = dataLockStatus.Status;
            datalock.TriageStatus = dataLockStatus.TriageStatus;
            datalock.ApprenticeshipUpdateId = dataLockStatus.ApprenticeshipUpdateId;
            datalock.IsResolved = dataLockStatus.IsResolved;
            datalock.EventStatus = dataLockStatus.EventStatus;

            _db.Value.DataLocks.Update(datalock);
        }

        await _db.Value.SaveChangesAsync();
        return result;
    }

    private async Task SetHasHadDataLockSuccess(long apprenticeshipId)
    {
        _logger.LogDebug("Setting HasHadDataLockSuccess for apprenticeship {ApprenticeshipId}", apprenticeshipId);

        var apprenticeship = await _db.Value.Apprenticeships.SingleOrDefaultAsync(x => x.Id == apprenticeshipId);

        if (apprenticeship == null || apprenticeship.HasHadDataLockSuccess)
        {
            return;
        }

        apprenticeship.HasHadDataLockSuccess = true;

        try
        {
            await _db.Value.SaveChangesAsync();
        }
        catch (InvalidOperationException exception )
        {
            _logger.LogError(exception, "InvalidOperationException occurred in SetHasHadDataLockSuccess - Apprenticeship {ApprenticeshipId}", apprenticeship);
        }
    }

    private async Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId)
    {
        _logger.LogInformation("Updating apprenticeship update {ApprenticeshipUpdateId} - to expired", apprenticeshipUpdateId);

        var apprenticeshipUpdate = _db.Value.ApprenticeshipUpdates
            .FirstOrDefault(x => x.Id == apprenticeshipUpdateId);
        
        if (apprenticeshipUpdate == null)
        {
            return;
        }

        var apprenticeship = _db.Value.Apprenticeships
            .FirstOrDefault(x => x.Id == apprenticeshipUpdate.ApprenticeshipId);

        if (apprenticeship != null)
        {
            apprenticeship.PendingUpdateOriginator = null;
            _db.Value.Apprenticeships.Update(apprenticeship);
        }

        apprenticeshipUpdate.Status = ApprenticeshipUpdateStatus.Expired;
        _db.Value.ApprenticeshipUpdates.Update(apprenticeshipUpdate);

        await _db.Value.SaveChangesAsync();
    }
}