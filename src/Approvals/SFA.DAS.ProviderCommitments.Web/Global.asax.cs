using System;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Extensions;

namespace SFA.DAS.ProviderCommitments.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog _log = new NLogLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        protected void Application_End()
        {
            _log.Info("Application closing");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();

            _log.Error(ex, ex.AggregateMessages());
        }
    }
}
