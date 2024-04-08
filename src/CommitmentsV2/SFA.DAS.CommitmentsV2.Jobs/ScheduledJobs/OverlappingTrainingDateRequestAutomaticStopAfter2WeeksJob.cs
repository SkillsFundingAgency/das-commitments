using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> _logger;
        private readonly IAutomaticStopOverlappingTrainingDateRequestsService _automaticStopOverlappingTrainingDateRequestsService;

        public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(IAutomaticStopOverlappingTrainingDateRequestsService automaticStopOverlappingTrainingDateRequestsService, ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger)
        {
            _automaticStopOverlappingTrainingDateRequestsService = automaticStopOverlappingTrainingDateRequestsService;
            _logger = logger;
        }

        public async Task StopApprenticeships([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");

            await _automaticStopOverlappingTrainingDateRequestsService.AutomaticallyStopOverlappingTrainingDateRequest();

            _logger.LogInformation("OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob - Finished");
        }
    }
}
