using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Globalization;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Configuration;
using System.Data.SqlClient;
using System.Data;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class FixCourseDataJobService : IFixCourseDataJobService
    {
        private readonly ILogger<FixCourseDataJobService> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IApprovalsOuterApiClient _outerApiClient;
        private readonly CommitmentPaymentsWebJobConfiguration _config;
        private readonly IFilterOutAcademicYearRollOverDataLocks _filterOutAcademicYearRollOverDataLocks;
        private readonly IList<DataLockErrorCode> _whiteList;

        private readonly DateTime _1718AcademicYearStartDate = new DateTime(2017, 08, 01);

        public FixCourseDataJobService(ILogger<FixCourseDataJobService> logger,
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
            _whiteList = new List<DataLockErrorCode>
            {
                DataLockErrorCode.Dlock03,
                DataLockErrorCode.Dlock04,
                DataLockErrorCode.Dlock05,
                DataLockErrorCode.Dlock06,
                DataLockErrorCode.Dlock07
            };
        }

        public async Task RunUpdate()
        {
            _logger.LogInformation("Retrieving last DataLock Event Id from repository");
            var lastId = await GetLastDataLockEventId();

            _logger.LogInformation($"Retrieving page of data from Payment Events Service since Event Id {lastId}");
            var stopwatch = Stopwatch.StartNew();

            var datalockStatusResponse = await _outerApiClient.GetWithRetry<GetDataLockStatusListResponse>(new GetDataLockEventsRequest(lastId));

            var page = datalockStatusResponse?.DataLockStatuses ?? new List<DataLockStatus>();

            stopwatch.Stop();
            _logger.LogInformation($"Response took {stopwatch.ElapsedMilliseconds}ms");

            if (!page.Any())
            {
                _logger.LogInformation("No data returned; exiting");
                return;
            }

            _logger.LogInformation($"{page.Count()} records returned in page");

            foreach (var dataLockStatus in page)
            {
                _logger.LogInformation($"Read datalock Apprenticeship {dataLockStatus.ApprenticeshipId} " +
                    $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode} and EventStatus: {dataLockStatus.EventStatus}");

                var datalockSuccess = dataLockStatus.ErrorCode == DataLockErrorCode.None;

                if (!datalockSuccess)
                {
                    ApplyErrorCodeWhiteList(dataLockStatus);
                }

                var is1617 = GetDateFromPriceEpisodeIdentifier(dataLockStatus) < _1718AcademicYearStartDate;
                if (is1617)
                {
                    _logger.LogInformation($"Data lock Event Id {dataLockStatus.DataLockEventId} pertains to 16/17 academic year and will be ignored");
                }

                if ((datalockSuccess || dataLockStatus.ErrorCode != DataLockErrorCode.None) && !is1617)
                {
                    var apprenticeship = await GetApprenticeship(dataLockStatus.ApprenticeshipId);

                    //temporarily ignore dlock7 & 9 combos until payments R14 fixes properly
                    if (dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07) && dataLockStatus.IlrEffectiveFromDate < apprenticeship.StartDate)
                    {
                        _logger.LogInformation($"Ignoring datalock for Apprenticeship #{dataLockStatus.ApprenticeshipId} Dlock07 with Effective Date before Start Date. Event Id {dataLockStatus.DataLockEventId}");
                    }
                    else
                    {
                        _logger.LogInformation($"Updating Apprenticeship {dataLockStatus.ApprenticeshipId} " +
                                     $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode}");

                        AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(apprenticeship, dataLockStatus);

                        try
                        {
                            await UpdateDataLockStatus(dataLockStatus);

                            await _filterOutAcademicYearRollOverDataLocks.Filter(dataLockStatus.ApprenticeshipId);
                        }
                        catch (RepositoryConstraintException ex) when (_config.IgnoreDataLockStatusConstraintErrors)
                        {
                            _logger.LogWarning(ex, $"Exception in DataLock updater");
                        }

                        if (datalockSuccess)
                        {
                            await SetHasHadDataLockSuccess(dataLockStatus.ApprenticeshipId);

                            var pendingUpdate = await GetPendingApprenticeshipUpdate(dataLockStatus.ApprenticeshipId);

                            if (pendingUpdate != null && (pendingUpdate.Cost != null || pendingUpdate.TrainingCode != null))
                            {
                                await ExpireApprenticeshipUpdate(pendingUpdate.Id);
                                _logger.LogInformation($"Pending ApprenticeshipUpdate {pendingUpdate.Id} expired due to successful data lock event {dataLockStatus.DataLockEventId}");
                            }
                        }
                    }
                }

                lastId = dataLockStatus.DataLockEventId;
            }
        }

        private void AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(Apprenticeship apprenticeship, DataLockStatus datalock)
        {
            if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn &&
                apprenticeship.StopDate == apprenticeship.StartDate)
            {
                _logger.LogInformation($"Auto-resolving datalock for Apprenticeship #{datalock.ApprenticeshipId} withdrawn effective at start date. Event Id {datalock.DataLockEventId}");

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
                _logger.LogInformation($"Skipping {skipped}");
            }

            dataLockStatus.ErrorCode = whitelisted;
        }

        private async Task<long> GetLastDataLockEventId()
        {
            var maxDataLockEventId = await _db.Value.DataLocks.MaxAsync(x => x.DataLockEventId);
            return maxDataLockEventId;
        }

        private static DateTime GetDateFromPriceEpisodeIdentifier(DataLockStatus dataLockStatus)
        {
            return
            DateTime.ParseExact(dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy", new CultureInfo("en-GB"));
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

        private async Task<long> UpdateDataLockStatus(DataLockStatus dataLockStatus)
        {
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

            var updatedRows = await _db.Value.SaveChangesAsync();
            return updatedRows;
        }

        private async Task SetHasHadDataLockSuccess(long apprenticeshipId)
        {
            _logger.LogDebug($"Setting HasHadDataLockSuccess for apprenticeship {apprenticeshipId}");

            var apprenticeships = await _db.Value.Apprenticeships
                .Where(x => x.Id == apprenticeshipId && !x.HasHadDataLockSuccess)
                .ToListAsync();

            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.HasHadDataLockSuccess = true;
                _db.Value.Apprenticeships.Update(apprenticeship);
            }

            await _db.Value.SaveChangesAsync();
        }

        private async Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId)
        {
            _logger.LogInformation($"Updating apprenticeship update {apprenticeshipUpdateId} - to expired");

            var apprenticeshipUpdate = _db.Value.ApprenticeshipUpdates
                .FirstOrDefault(x => x.Id == apprenticeshipUpdateId);
            if (apprenticeshipUpdate == null)
                return;

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
}