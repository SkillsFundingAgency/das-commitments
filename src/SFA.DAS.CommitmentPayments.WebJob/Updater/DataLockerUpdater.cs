using System;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public class DataLockerUpdater : IDataLockUpdater
    {
        private readonly ILog _logger;

        private readonly IPaymentEvents _paymentEventsSerivce;

        public DataLockerUpdater(ILog logger, IPaymentEvents paymentEventsService)
        {
            if(logger==null)
                throw new ArgumentNullException(nameof(ILog));
            if (paymentEventsService== null)
                throw new ArgumentNullException(nameof(IPaymentEvents));

            _logger = logger;
            _paymentEventsSerivce = paymentEventsService;
        }

        public async Task RunUpdate()
        {
            var result = await _paymentEventsSerivce.GetDataLockEvents();
            //...
        }
    }
}
