using System.Threading.Tasks;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class DummyTask : IDummyTask
    {

        private readonly ILog _logger;
        public DummyTask(ILog logger)
        {
            _logger = logger;
        }
        public async Task RunUpdate()
        {
            _logger.Trace($"{nameof(DummyTask)} executing...");
            await Task.FromResult(0);
        }
    }
}