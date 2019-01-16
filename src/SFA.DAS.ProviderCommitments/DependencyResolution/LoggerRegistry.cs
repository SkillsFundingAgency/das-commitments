using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.DependencyResolution
{
    public class LoggerRegistry : Registry
    {
        public LoggerRegistry()
        {
            For<ILoggerFactory>().Use(() => new LoggerFactory().AddNLog()).Singleton();
            For<ILogger>().Use(c => c.GetInstance<ILoggerFactory>().CreateLogger(c.ParentType));
            For(typeof(ILogger<>)).Use(typeof(Logger<>));
        }
    }
}