﻿using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerTransferRequestPendingNotificationJob
{
    private readonly IEmployerTransferRequestPendingEmailService _employerTransferRequestPendingEmailService;
    private readonly ILogger<EmployerTransferRequestPendingNotificationJob> _logger;

    public EmployerTransferRequestPendingNotificationJob(IEmployerTransferRequestPendingEmailService employerTransferRequestPendingEmailService, ILogger<EmployerTransferRequestPendingNotificationJob> logger)
    {
        _employerTransferRequestPendingEmailService = employerTransferRequestPendingEmailService;
        _logger = logger;
    }

    public async Task Notify([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
    {
        _logger.LogInformation($"EmployerTransferRequestPendingNotificationJob - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");

        await _employerTransferRequestPendingEmailService.SendEmployerTransferRequestPendingNotifications();

        _logger.LogInformation("EmployerTransferRequestPendingNotificationJob - Finished");
    }
}