using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class AcademicYearEndExpiryProcessorJob
    {
        private readonly IAcademicYearEndExpiryProcessorService _academicYearProcessor;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly ILogger<AcademicYearEndExpiryProcessorJob> _logger;

        private string _jobId;

        public AcademicYearEndExpiryProcessorJob(
            IAcademicYearEndExpiryProcessorService academicYearProcessor,
            ICurrentDateTime currentDateTime,
            IAcademicYearDateProvider academicYearProvider,
            ILogger<AcademicYearEndExpiryProcessorJob> logger)
        {
            _academicYearProcessor = academicYearProcessor;
            _currentDateTime = currentDateTime;
            _academicYearProvider = academicYearProvider;
            _logger = logger;
            _jobId = $"AcademicYearEnd.WebJob.{DateTime.UtcNow.Ticks}";
        }

        public async Task Run([TimerTrigger("0 0 1 1 11 *", RunOnStartup = true)] TimerInfo timer)
        {
            if (_currentDateTime.UtcNow < _academicYearProvider.LastAcademicYearFundingPeriod)
            {
                _logger.LogInformation($"The {nameof(AcademicYearEndExpiryProcessorService)} job cannot run before last academic year funding period. ({_academicYearProvider.LastAcademicYearFundingPeriod}) - current date time {_currentDateTime.UtcNow} , JobId: {_jobId}");
                return;
            }

            try
            {
                await _academicYearProcessor.ExpireApprenticeshipUpdates($"{_jobId}.ChangeOfCircs")
                    .ContinueWith(t => WhenDone(t, _logger, "ChangeOfCircs"));


                await _academicYearProcessor.ExpireDataLocks($"{_jobId}.DataLocks")
                    .ContinueWith(t => WhenDone(t, _logger, "DataLocks"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running {nameof(AcademicYearEndExpiryProcessorService)}.WebJob");
            }
        }

        private void WhenDone(Task task, ILogger<AcademicYearEndExpiryProcessorJob> logger, string identifier)
        {
            if (task.IsFaulted)
                logger.LogError(task.Exception, $"Error running {identifier} AcademicYearEndProcessor.WebJob, JobId: {_jobId}.{identifier}");
            else
                logger.LogInformation($"Successfully ran AcademicYearEndProcessor.WebJob for {identifier}, JobId: {_jobId}.{identifier}");
        }
    }
}