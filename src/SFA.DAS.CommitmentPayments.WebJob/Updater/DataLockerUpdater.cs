using System;
using System.Threading.Tasks;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public class DataLockerUpdater : IDataLockUpdater
    {
        private readonly ILog _logger;

        public DataLockerUpdater(ILog logger)
        {
            if(logger==null)
                throw new ArgumentNullException(nameof(ILog));

            _logger = logger;
        }

        public async Task RunUpdate()
        {
            //...
        }
    }
}
