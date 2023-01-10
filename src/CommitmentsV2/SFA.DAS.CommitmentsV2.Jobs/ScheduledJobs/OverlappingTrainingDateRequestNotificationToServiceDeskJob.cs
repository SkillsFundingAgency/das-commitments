using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestNotificationToServiceDeskJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskJob> _logger;
        private readonly IMediator _mediator;

        public OverlappingTrainingDateRequestNotificationToServiceDeskJob(IMediator mediator, ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Notify([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDNotificationToServiceDeskJobSchedule%", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestNotificationToServiceDeskJob");
            await _mediator.Send(new OverlappingTrainingDateRequestNotificationToServiceDeskCommand());
        }
    }
}
