using SFA.DAS.Commitments.Notification.WebJob.DependencyResolution;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var container = IoC.Initialize();

            var logger = container.GetInstance<ILog>();
            var updater = container.GetInstance<INotificationSummary>();
        }
    }
}
