using System;
using System.Web.Http;
using System.Web.Mvc;
using NLog;
using NLog.Targets;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static ILog _logger = DependencyResolver.Current.GetService<ILog>();
        private static RedisTarget _redisTarget; // Required to ensure assembly is copied to output.

        protected void Application_Start()
        {
            _logger.Info("Starting Commitments Api Application");
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_End()
        {
            _logger.Info("Stopping Events Api Application");
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError().GetBaseException();

            _logger.Error(ex, "Unhandled exception");
        }
    }
}
