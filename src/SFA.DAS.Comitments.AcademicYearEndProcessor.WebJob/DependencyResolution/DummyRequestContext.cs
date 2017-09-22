using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.DependencyResolution
{
    public class DummyRequestContext : IRequestContext
    {
        public string Url { get; }

        public string IpAddress { get; }
    }
}