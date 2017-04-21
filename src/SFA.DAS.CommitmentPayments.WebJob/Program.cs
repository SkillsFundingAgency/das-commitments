using System;
using SFA.DAS.CommitmentPayments.WebJob.DependencyResolution;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = IoC.Initialize();

            var logger = container.GetInstance<ILog>();
            var updater = container.GetInstance<IDataLockUpdater>();

            logger.Info("Starting CommitmentPayments.WebJob");

            try
            {
                updater.RunUpdate().Wait();
            }
            catch(Exception ex)
            {
               logger.Error(ex, "Error running CommitmentPayments.WebJob");
            }
        }
    }
}
