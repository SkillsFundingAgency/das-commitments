using System;
using System.Threading.Tasks;

using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.DependencyResolution;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob
{
    class Program
    {
        static void Main()
        {
            var container = IoC.Initialize();

            var logger = container.GetInstance<ILog>();
            var updater = container.GetInstance<IAcademicYearEndExpiryProcessor>();

            logger.Info($"Starting {nameof(AcademicYearEndExpiryProcessor)}.WebJob");

            try
            {
                /*
                 *  What ApprenticeshipUpdateStatus should we use?
                    Superceded or NEW?
                    Do we need expired date?

                    Do we want to pull all ApprenticehsipUpdates back and then update one by one? (Easy loggin)
                    Or do we want to do everything in a stored proc?

                --------------------
                    Do we want to not run the job between AY-end and cutoff date?
                    More logging needed?
                 */
                var t1 = updater.RunChangeOfCircUpdate()
                    .ContinueWith(t => WhenDone(t, logger, "ChangeOfCircs"));

                var t2 = updater.RunUpdate()
                    .ContinueWith(t => WhenDone(t, logger, "ChangeOfCircs"));

                Task.WaitAll(t1, t2);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error running {nameof(AcademicYearEndExpiryProcessor)}.WebJob");
            }
        }

        private static void WhenDone(Task task, ILog logger, string identifier)
        {
            if (task.IsFaulted)
                logger.Error(task.Exception, $"Error running {identifier} AcademicYearEndProcessor.WebJob");
            else
                logger.Info($"Successfully ran AcademicYearEndProcessor.WebJob for {identifier}");
        }
    }
}
