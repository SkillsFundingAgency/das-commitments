using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.ProviderCommitments.Extensions;

namespace SFA.DAS.ProviderCommitments.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //DependencyResolver.Current.GetService<IStartup>().StartAsync().GetAwaiter().GetResult();
        }

        protected void Application_End()
        {
            //DependencyResolver.Current.GetService<IStartup>().StopAsync().GetAwaiter().GetResult();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var logger = DependencyResolver.Current.GetService<ILogger<MvcApplication>>();

            logger.LogError(ex, ex.AggregateMessages());
        }
    }
}
