using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class AddEpaToApprenticeshipsJob(IAddEpaToApprenticeshipService addEpaToApprenticeshipService, ILogger<AddEpaToApprenticeshipsJob> logger)
{
    public async Task Notify([TimerTrigger("0 */20 * * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("AddEpaToApprenticeshipsJob - Started {Time}", timer?.IsPastDue ?? false ? " later than expected" : string.Empty);

        await addEpaToApprenticeshipService.Update();

        logger.LogInformation("AddEpaToApprenticeshipsJob - Finished");
    }
}