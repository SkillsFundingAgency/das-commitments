using System;
using System.Web.Http;
using NLog;
using NLog.Targets;

namespace SFA.DAS.Commitments.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static RedisTarget _redisTarget; // Required to ensure assembly is copied to output.

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            Logger.Error(exception);
        }
    }
}
