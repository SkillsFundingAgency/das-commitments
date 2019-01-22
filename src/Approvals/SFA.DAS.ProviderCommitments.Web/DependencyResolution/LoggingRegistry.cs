using System.Web;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.ProviderCommitments.Web.DependencyResolution
{
    public class LoggingRegistry : StructureMap.Registry
    {
        public LoggingRegistry()
        {
            For<ILoggingContext>().Use(x => new WebLoggingContext(new HttpContextWrapper(HttpContext.Current)));
            For<ILog>().Use(x => new NLogLogger(
                x.ParentType,
                x.GetInstance<ILoggingContext>(),
                null)).AlwaysUnique();
        }
    }
}