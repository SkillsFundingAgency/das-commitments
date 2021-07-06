using System.Web.Http;
using System.Web.Mvc;

using NLog.Targets;
using SFA.DAS.NLog.Logger;
using Microsoft.ApplicationInsights.Extensibility;
using System.Configuration;

namespace SFA.DAS.Commitments.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static ILog Logger = new NLogLogger();
        
#pragma warning disable 0169
        private static RedisTarget _redisTarget; // Required to ensure assembly is copied to output.
#pragma warning disable 0169

        protected void Application_Start()
        {
            Logger.Info("Starting Commitments Api Application");
            FilterConfig.RegisterGlobalFilters(GlobalConfiguration.Configuration.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"] ?? "";
        }

        protected void Application_End()
        {
            Logger.Info("Stopping Commitments Api Application");
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError().GetBaseException();

            Logger.Error(ex, "Unhandled exception");
        }
    }
}
