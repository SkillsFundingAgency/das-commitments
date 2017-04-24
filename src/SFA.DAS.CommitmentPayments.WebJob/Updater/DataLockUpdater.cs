using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public class DataLockUpdater : IDataLockUpdater
    {
        private readonly ILog _logger;

        private readonly IPaymentEvents _paymentEventsSerivce;
        private readonly IDataLockRepository _dataLockRepository;

        public DataLockUpdater(ILog logger, IPaymentEvents paymentEventsService, IDataLockRepository dataLockRepository)
        {
            if(logger==null)
                throw new ArgumentNullException(nameof(ILog));
            if (paymentEventsService== null)
                throw new ArgumentNullException(nameof(IPaymentEvents));
            if(dataLockRepository==null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _logger = logger;
            _paymentEventsSerivce = paymentEventsService;
            _dataLockRepository = dataLockRepository;
        }

        public async Task RunUpdate()
        {
            var lastId = await _dataLockRepository.GetLastDataLockEventId();

            while (true)
            {
                var page = (await _paymentEventsSerivce.GetDataLockEvents(lastId)).ToList();

                if (!page.Any())
                {
                    break;
                }

                foreach (var dataLockStatus in page)
                {
                    await _dataLockRepository.UpdateDataLockStatus(dataLockStatus);
                    lastId = dataLockStatus.DataLockEventId;
                }
            }
        }
    }
}
