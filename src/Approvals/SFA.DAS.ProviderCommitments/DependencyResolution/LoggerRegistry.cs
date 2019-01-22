using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.DependencyResolution
{
    public class LoggerRegistry : Registry
    {
        public LoggerRegistry()
        {
            For<ILog>().Use(x => new NLogLogger(x.ParentType, null, null)).AlwaysUnique();
        }
    }
}