﻿using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class DataLockUpdaterJobs
{
    private readonly ILogger<DataLockUpdaterJobs> _logger;
    private readonly IDataLockUpdaterService _dataLockUpdaterServicer;

    public DataLockUpdaterJobs(ILogger<DataLockUpdaterJobs> logger, IDataLockUpdaterService dataLockUpdaterServicer)
    {
        _logger = logger;
        _dataLockUpdaterServicer = dataLockUpdaterServicer;
    }

    public async Task Update([TimerTrigger("*/30 * * * * *", RunOnStartup = false)] TimerInfo timer)
    {
        _logger.LogInformation($"DataLockUpdaterJobs - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");

        await _dataLockUpdaterServicer.RunUpdate();

        _logger.LogInformation("DataLockUpdaterJobs - Finished");
    }
}