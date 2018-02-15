using System;
using System.Threading;
using SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        static void Main()
        {
            // how to remote debug in azure - there's no working guide on the web (that I could find) for this!
            // start job with sleep (to give you time to attach)
            // Thread.Sleep(1000 * 120);
            // attach to process...
            // connection target = sfadascommitmentshostwebjob20180214083531.scm.azurewebsites.net:4022
            // (i.e. websitename, add .scm & add port 4022
            // enter credentials (from webjob properties in portal)
            // show processes for all users

            var container = IoC.Initialize();

            var logger = container.GetInstance<ILog>();
            var addEpaToApprenticeship = container.GetInstance<IAddEpaToApprenticeships>();

            logger.Info("Starting AddEpaToApprenticeships.WebJob");

            // we don't need to use JobHost - the other web jobs in the solution don't. here's why...
            // https://stackoverflow.com/questions/25811719/azure-webjobs-sdk-in-what-scenarios-is-creation-of-a-jobhost-object-required

            try
            {
                addEpaToApprenticeship.Update().Wait();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error running AddEpaToApprenticeship.WebJob");
            }
        }
    }
}
