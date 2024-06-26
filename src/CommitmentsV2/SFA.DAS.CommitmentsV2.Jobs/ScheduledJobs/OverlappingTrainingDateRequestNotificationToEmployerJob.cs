﻿using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class OverlappingTrainingDateRequestNotificationToEmployerJob
{
    private readonly ILogger<OverlappingTrainingDateRequestNotificationToEmployerJob> _logger;
    private readonly IMediator _mediator;

    public OverlappingTrainingDateRequestNotificationToEmployerJob(IMediator mediator, ILogger<OverlappingTrainingDateRequestNotificationToEmployerJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Notify([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDNotificationToEmployerJobSchedule%", RunOnStartup = false)] TimerInfo timer)
    {
        _logger.LogInformation("Starting OverlappingTrainingDateRequestNotificationToEmployerJob");
        await _mediator.Send(new OverlappingTrainingDateRequestNotificationToEmployerCommand());
    }
}