using SFA.DAS.Commitments.Notification.WebJob.Configuration;
using SFA.DAS.Commitments.Notification.WebJob.DependencyResolution;
using SFA.DAS.NLog.Logger;
using System;
using System.Threading.Tasks;

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
            var config = container.GetInstance<CommitmentNotificationConfiguration>();
            var notificationJob = container.GetInstance<INotificationJob>();

            if (!config.EnableJob)
            {
                logger.Info("CommitmentNotification.WebJob job is turned off");
                return;
            }

            logger.Trace("Starting CommitmentNotification.WebJob");

            var t1 = notificationJob.RunEmployerNotification()
                .ContinueWith(t => OnException(t, logger, "Employer"));
            var t2 = notificationJob.RunProviderNotification()
                .ContinueWith(t => OnException(t, logger, "Provider"));

            Task.WaitAll(t1, t2);
        }

        private static void OnException(Task task, ILog logger, string identifier)
        {
            if(task.IsFaulted)
                logger.Error(task.Exception, $"Error running {identifier} CommitmentNotification.WebJob");
        }
    }
}
