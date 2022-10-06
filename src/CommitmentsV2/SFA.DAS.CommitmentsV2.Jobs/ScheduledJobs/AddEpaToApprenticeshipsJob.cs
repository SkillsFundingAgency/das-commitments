using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class AddEpaToApprenticeshipsJob
    {
        private readonly IAddEpaToApprenticeshipService _addEpaToApprenticeshipService;
        private readonly ILogger<AddEpaToApprenticeshipsJob> _logger;

        public AddEpaToApprenticeshipsJob(IAddEpaToApprenticeshipService addEpaToApprenticeshipService, ILogger<AddEpaToApprenticeshipsJob> logger)
        {
            _addEpaToApprenticeshipService = addEpaToApprenticeshipService;
            _logger = logger;
        }

        public async Task Notify([TimerTrigger("0 */20 * * * *", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation($"AddEpaToApprenticeshipsJob - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");

            await _addEpaToApprenticeshipService.Update();

            _logger.LogInformation("AddEpaToApprenticeshipsJob - Finished");
        }
    }
}
