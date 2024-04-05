using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> _logger;
        private readonly IMediator _mediator;

        public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(IMediator mediator, ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Notify([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");
            await _mediator.Send(new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand());
        }
    }
}
