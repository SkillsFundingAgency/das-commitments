using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestNotificationToEmployerJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestNotificationToEmployerJob> _logger;
        private readonly IMediator _mediator;

        public OverlappingTrainingDateRequestNotificationToEmployerJob(IMediator mediator, ILogger<OverlappingTrainingDateRequestNotificationToEmployerJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Notify([TimerTrigger("%OverlappingTrainingDateRequestNotificationToEmployerJobSchdule%", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestNotificationToServiceDeskJob");
            await _mediator.Send(new OverlappingTrainingDateRequestNotificationToServiceDeskCommand());
        }
    }
}
