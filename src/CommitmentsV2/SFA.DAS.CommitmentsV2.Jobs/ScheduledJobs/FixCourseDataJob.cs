using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
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
        private readonly IFeatureTogglesService<FeatureToggle> _featureTogglesService;

        public FixCourseDataJob(ILogger<FixCourseDataJob> logger, IFixCourseDataJobService fixCourseDataService, IFeatureTogglesService<FeatureToggle> featureTogglesService)
        {
            _logger = logger;
            _fixCourseDataService = fixCourseDataService;
            _featureTogglesService = featureTogglesService;
        }

        public async Task Update([TimerTrigger("*/30 * * * * *", RunOnStartup = true)] TimerInfo timer)
        {
            var fixCourseDataJobToggle = _featureTogglesService.GetFeatureToggle("FixCourseDataJob").IsEnabled;

            _logger.LogInformation($"FixCourseDataJob - Enabled: {fixCourseDataJobToggle}");

            if (fixCourseDataJobToggle)
            {
                _logger.LogInformation($"FixCourseDataJob - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");
                await _fixCourseDataService.RunUpdate();
                _logger.LogInformation("FixCourseDataJob - Finished");
            }

        }
    }
}