using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob
{
    public class Job
    {
        private readonly IAcademicYearEndExpiryProcessor _academicYearProcessor;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly ILogger<Job> _logger;

        private string _jobId;

        public Job(
            IAcademicYearEndExpiryProcessor academicYearProcessor,
            ICurrentDateTime currentDateTime,
            IAcademicYearDateProvider academicYearProvider,
            ILogger<Job> logger)
        {
            _academicYearProcessor = academicYearProcessor;
            _currentDateTime = currentDateTime;
            _academicYearProvider = academicYearProvider;
            _logger = logger;
            _jobId = $"AcademicYearEnd.WebJob.{DateTime.UtcNow.Ticks}";
        }

        public void Run([TimerTrigger("0 18 23 08 09 *", RunOnStartup = false)] TimerInfo timer)
        {
            if (_currentDateTime.UtcNow < _academicYearProvider.LastAcademicYearFundingPeriod)
            {
                _logger.LogInformation($"The {nameof(AcademicYearEndExpiryProcessor)} job cannot run before last academic year funding period. ({_academicYearProvider.LastAcademicYearFundingPeriod}) - current date time {_currentDateTime.UtcNow} , JobId: {_jobId}");
                return;
            }

            try
            {
                var t1 = _academicYearProcessor.RunApprenticeshipUpdateJob($"{_jobId}.ChangeOfCircs")
                    .ContinueWith(t => WhenDone(t, _logger, "ChangeOfCircs"));

                var t2 = _academicYearProcessor.RunDataLock($"{_jobId}.DataLocks")
                    .ContinueWith(t => WhenDone(t, _logger, "DataLocks"));

                Task.WaitAll(t1, t2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running {nameof(AcademicYearEndExpiryProcessor)}.WebJob");
            }
        }

        private void WhenDone(Task task, ILogger<Job> logger, string identifier)
        {
            if (task.IsFaulted)
                logger.LogError(task.Exception, $"Error running {identifier} AcademicYearEndProcessor.WebJob, JobId: {_jobId}.{identifier}");
            else
                logger.LogError($"Successfully ran AcademicYearEndProcessor.WebJob for {identifier}, JobId: {_jobId}.{identifier}");
        }
    }
}