using System;
using System.Threading.Tasks;

using NLog;

using SFA.DAS.Commitments.Notification.WebJob.Configuration;
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
            /* All the notifications jobs have been migrated to V2 code in CON-4584 / CON-4709; but currently disabled
               here - this code can be removed in the general V1 code cleanup
            
            var container = IoC.Initialize();
            var logger = container.GetInstance<ILog>();
            var config = container.GetInstance<CommitmentNotificationConfiguration>();
            var notificationJob = container.GetInstance<INotificationJob>();
            var notificationJobId = $"Notification.WJ.{DateTime.UtcNow.Ticks}";
            MappedDiagnosticsLogicalContext.Set(Constants.HeaderNameSessionCorrelationId, notificationJobId);

            if (!config.EnableJob)
            {
                logger.Info($"CommitmentNotification.WebJob job is turned off, JobId: {notificationJobId}");
                return;
            }

            logger.Trace($"Starting CommitmentNotification.WebJob, JobId: {notificationJobId}");

            var t1 = notificationJob.RunEmployerAlertSummaryNotification($"{notificationJobId}.Employer")
                .ContinueWith(t => WhenDone(t, logger, "Employer"));
            var t2 = notificationJob.RunProviderAlertSummaryNotification($"{notificationJobId}.Provider")
                .ContinueWith(t => WhenDone(t, logger, "Provider"));
            var t3 = notificationJob.RunSendingEmployerTransferRequestNotification($"{notificationJobId}.SendingEmployer")
                .ContinueWith(t => WhenDone(t, logger, "SendingEmployer"));

            Task.WaitAll(t1, t2, t3);
            
            */
        }

        private static void WhenDone(Task task, ILog logger, string identifier)
        {
            if(task.IsFaulted)
                logger.Error(task.Exception, $"Error running {identifier} CommitmentNotification.WebJob");
            else
                logger.Info($"Successfully ran CommitmentNotification.WebJob for {identifier}");
        }
    }
}
