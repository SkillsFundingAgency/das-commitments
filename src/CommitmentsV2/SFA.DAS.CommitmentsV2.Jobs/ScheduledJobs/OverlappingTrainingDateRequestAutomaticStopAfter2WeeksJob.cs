using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.UnitOfWork.DependencyResolution.StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(
        IAutomaticStopOverlappingTrainingDateRequestsService automaticStopOverlappingTrainingDateRequestsService,
        ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger,
        IUnitOfWorkScope unitOfWorkScope)
    {
        public async Task StopApprenticeships([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo timer)
        {
            logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");

            await unitOfWorkScope.RunAsync(async sp =>
            {    
                await automaticStopOverlappingTrainingDateRequestsService.AutomaticallyStopOverlappingTrainingDateRequest();
            });
        

            logger.LogInformation("OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob - Finished");
        }
    }
}
