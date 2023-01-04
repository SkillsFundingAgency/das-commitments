using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class FixCourseDataJob
    {
        private readonly ILogger<FixCourseDataJob> _logger;
        private readonly IFixCourseDataJobService _fixCourseDataService;

        public FixCourseDataJob(ILogger<FixCourseDataJob> logger, IFixCourseDataJobService fixCourseDataService)
        {
            _logger = logger;
            _fixCourseDataService = fixCourseDataService;
        }

        public async Task Update([TimerTrigger("*/30 * * * * *", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation($"DataLockUpdaterJobs - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");

            await _fixCourseDataService.RunUpdate();

            _logger.LogInformation("DataLockUpdaterJobs - Finished");
        }
    }
}