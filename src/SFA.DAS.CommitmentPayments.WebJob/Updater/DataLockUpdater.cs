using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentPayments.WebJob.Configuration;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Commitments.Domain.Exceptions;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public class DataLockUpdater : IDataLockUpdater
    {
        private readonly ILog _logger;

        private readonly IPaymentEvents _paymentEventsSerivce;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly CommitmentPaymentsConfiguration _config;
        private readonly IFilterOutAcademicYearRollOverDataLocks _filterAcademicYearRolloverDataLocks;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly IList<DataLockErrorCode> _whiteList;

        public DataLockUpdater(ILog logger,
            IPaymentEvents paymentEventsService,
            IDataLockRepository dataLockRepository,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            CommitmentPaymentsConfiguration config,
            IFilterOutAcademicYearRollOverDataLocks filter,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _logger = logger;
            _paymentEventsSerivce = paymentEventsService;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _config = config;
            _filterAcademicYearRolloverDataLocks = filter;
            _apprenticeshipRepository = apprenticeshipRepository;

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
            _logger.Info("Retrieving last DataLock Event Id from repository");
            var lastId = await _dataLockRepository.GetLastDataLockEventId();

            while (true)
            {
                _logger.Info($"Retrieving page of data from Payment Events Service since Event Id {lastId}");
                var stopwatch = Stopwatch.StartNew();
                var page = (await _paymentEventsSerivce.GetDataLockEvents(lastId)).ToList();
                stopwatch.Stop();
                _logger.Info($"Response took {stopwatch.ElapsedMilliseconds}ms");

                if (!page.Any())
                {
                    _logger.Info("No data returned; exiting");
                    break;
                }

                _logger.Info($"{page.Count} records returned in page");

                foreach (var dataLockStatus in page)
                {
                    _logger.Info($"Read datalock Apprenticeship {dataLockStatus.ApprenticeshipId} " +
                        $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode} and EventStatus: {dataLockStatus.EventStatus}");

                    var datalockSuccess = dataLockStatus.ErrorCode == DataLockErrorCode.None;

                    if (!datalockSuccess)
                    {
                        ApplyErrorCodeWhiteList(dataLockStatus);
                    }

                    if (datalockSuccess || dataLockStatus.ErrorCode != DataLockErrorCode.None)
                    {
                        _logger.Info($"Updating Apprenticeship {dataLockStatus.ApprenticeshipId} " +
                             $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode}");

                        await AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(dataLockStatus);

                        try
                        {
                            await _dataLockRepository.UpdateDataLockStatus(dataLockStatus);

                            await _filterAcademicYearRolloverDataLocks.Filter(dataLockStatus.ApprenticeshipId);
                        }
                        catch(RepositoryConstraintException ex) when (_config.IgnoreDataLockStatusConstraintErrors)
                        {
                            _logger.Warn(ex, $"Exception in DataLock updater");
                        }

                        if (datalockSuccess)
                        {
                            await _apprenticeshipRepository.SetHasHadDataLockSuccess(dataLockStatus.ApprenticeshipId);

                            var pendingUpdate = await
                             _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(dataLockStatus.ApprenticeshipId);

                            if (pendingUpdate != null && (pendingUpdate.Cost != null || pendingUpdate.TrainingCode != null))
                            {
                                await _apprenticeshipUpdateRepository.ExpireApprenticeshipUpdate(pendingUpdate.Id);
                                _logger.Info($"Pending ApprenticeshipUpdate {pendingUpdate.Id} expired due to successful data lock event {dataLockStatus.DataLockEventId}");
                            }
                        }
                    }

                    lastId = dataLockStatus.DataLockEventId;
                }
            }
        }

        private async Task AutoResolveDataLockIfApprenticeshipStoppedAndBackdated(DataLockStatus datalock)
        {
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(datalock.ApprenticeshipId);

            if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn &&
                apprenticeship.StopDate == apprenticeship.StartDate)
            {
                _logger.Info($"Auto-resolving datalock for Apprenticeship #{datalock.ApprenticeshipId} withdrawn effective at start date. Event Id {datalock.DataLockEventId}");

                datalock.IsResolved = true;
            }
        }

        private void ApplyErrorCodeWhiteList(DataLockStatus dataLockStatus)
        {
            var whitelisted = DataLockErrorCode.None;
            var skipped = DataLockErrorCode.None;

            foreach(DataLockErrorCode errorCode in Enum.GetValues(typeof(DataLockErrorCode)))
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
                _logger.Info($"Skipping {skipped}");
            }

            dataLockStatus.ErrorCode = whitelisted;
        }
    }
}
