using System;
using System.Threading.Tasks;

using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob
{
    public class Job
    {
        private readonly IAcademicYearEndExpiryProcessor _academicYearProcessor;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly ILog _logger;

        private string _jobId;

        public Job(
            IAcademicYearEndExpiryProcessor academicYearProcessor,
            ICurrentDateTime currentDateTime,
            IAcademicYearDateProvider academicYearProvider,
            ILog logger)
        {
            _academicYearProcessor = academicYearProcessor;
            _currentDateTime = currentDateTime;
            _academicYearProvider = academicYearProvider;
            _logger = logger;
            _jobId = $"AcademicYearEnd.WebJob.{DateTime.UtcNow.Ticks}";
        }

        public void Run()
        {
            if (_currentDateTime.Now < _academicYearProvider.LastAcademicYearFundingPeriod)
            {
                _logger.Info($"The {nameof(AcademicYearEndExpiryProcessor)} job cannot run before last academic year funding period. ({_academicYearProvider.LastAcademicYearFundingPeriod}) - current date time {_currentDateTime.Now} , JobId: {_jobId}");
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
                _logger.Error(ex, $"Error running {nameof(AcademicYearEndExpiryProcessor)}.WebJob");
            }
        }

        private void WhenDone(Task task, ILog logger, string identifier)
        {
            if (task.IsFaulted)
                logger.Error(task.Exception, $"Error running {identifier} AcademicYearEndProcessor.WebJob, JobId: {_jobId}.{identifier}");
            else
                logger.Info($"Successfully ran AcademicYearEndProcessor.WebJob for {identifier}, JobId: {_jobId}.{identifier}");
        }
    }
}