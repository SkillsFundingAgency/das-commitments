using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //todo: add config to confluence page
            //todo: db will need to be deployed
            //todo: add schedule - chron line in root? check with devops

            var container = IoC.Initialize();

            var logger = container.GetInstance<ILog>();
            var addEpaToApprenticeship = container.GetInstance<IAddEpaToApprenticeships>();

            logger.Info("Starting AddEpaToApprenticeships.WebJob");

            // do we need to use WebJob?
            // https://stackoverflow.com/questions/25811719/azure-webjobs-sdk-in-what-scenarios-is-creation-of-a-jobhost-object-required

            //var config = new JobHostConfiguration();

            //if (config.IsDevelopment)
            //{
            //    config.UseDevelopmentSettings();
            //}

            //var host = new JobHost(config);
            //// The following code will invoke a function called ManualTrigger and 
            //// pass in data (value in this case) to the function
            //host.Call(typeof(Functions).GetMethod("ManualTrigger"), new { value = 20 });

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
