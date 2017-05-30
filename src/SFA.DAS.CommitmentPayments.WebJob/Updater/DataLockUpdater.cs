using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public class DataLockUpdater : IDataLockUpdater
    {
        private readonly ILog _logger;

        private readonly IPaymentEvents _paymentEventsSerivce;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;

        private readonly IList<DataLockErrorCode> _whiteList;

        public DataLockUpdater(ILog logger, IPaymentEvents paymentEventsService, IDataLockRepository dataLockRepository, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository)
        {
            if(logger==null)
                throw new ArgumentNullException(nameof(ILog));
            if (paymentEventsService== null)
                throw new ArgumentNullException(nameof(IPaymentEvents));
            if(dataLockRepository==null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if(apprenticeshipUpdateRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipUpdateRepository));

            _logger = logger;
            _paymentEventsSerivce = paymentEventsService;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;

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
                        $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode}");

                    ApplyErrorCodeWhiteList(dataLockStatus);

                    if (dataLockStatus.ErrorCode != DataLockErrorCode.None)
                    {
                        _logger.Info($"Updating Apprenticeship {dataLockStatus.ApprenticeshipId} " +
                             $"Event Id {dataLockStatus.DataLockEventId} Status {dataLockStatus.ErrorCode}");

                        var pendingApprenticeshipUpdate =
                        await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(dataLockStatus.ApprenticeshipId);

                        if (pendingApprenticeshipUpdate != null && pendingApprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                        {
                            await _apprenticeshipUpdateRepository.SupercedeApprenticeshipUpdate(dataLockStatus.ApprenticeshipId);
                        }
                       
                        await _dataLockRepository.UpdateDataLockStatus(dataLockStatus);
                    }

                    lastId = dataLockStatus.DataLockEventId;
                }
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
