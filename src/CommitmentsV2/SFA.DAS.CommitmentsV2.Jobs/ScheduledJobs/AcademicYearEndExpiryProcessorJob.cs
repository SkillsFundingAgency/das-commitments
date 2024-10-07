using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class AcademicYearEndExpiryProcessorJob
{
    private readonly IAcademicYearEndExpiryProcessorService _academicYearProcessor;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAcademicYearDateProvider _academicYearProvider;
    private readonly ILogger<AcademicYearEndExpiryProcessorJob> _logger;

    private readonly string _jobId;

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

    public async Task Run([TimerTrigger("0 0 1 1 11 *", RunOnStartup = false)] TimerInfo timer)
    {
        if (_currentDateTime.UtcNow < _academicYearProvider.LastAcademicYearFundingPeriod)
        {
            _logger.LogInformation("The {TypeName} job cannot run before last academic year funding period. ({LastAcademicYearFundingPeriod}) - current date time {CurrentDateTime} , JobId: {JobId}",
                nameof(AcademicYearEndExpiryProcessorService),
                _academicYearProvider.LastAcademicYearFundingPeriod,
                _currentDateTime.UtcNow,
                _jobId);

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
            _logger.LogError(ex, "Error running {TypeName}.WebJob", nameof(AcademicYearEndExpiryProcessorService));
        }
    }

    private void WhenDone(Task task, ILogger<AcademicYearEndExpiryProcessorJob> logger, string identifier)
    {
        if (task.IsFaulted)
        {
            logger.LogError(task.Exception, "Error running {Identifier} AcademicYearEndProcessor.WebJob, JobId: {JobId}", identifier, _jobId);
        }
        else
        {
            logger.LogInformation("Successfully ran AcademicYearEndProcessor.WebJob for {Identifier}, JobId: {JobId}", identifier, _jobId);
        }
    }
}