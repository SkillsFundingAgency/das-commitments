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
            var job = container.GetInstance<Job>();

            logger.Info($"Starting {nameof(AcademicYearEndExpiryProcessor)}.WebJob");

            //job.Run();
        }   
    }
}
