using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task Notify([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestNotificationToServiceDeskJob");
        }
    }
}
