using System.Diagnostics.CodeAnalysis;
using System.Web.Http;

namespace SFA.DAS.Commitments.Support.SubSite
{
    [ExcludeFromCodeCoverage]
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new {id = RouteParameter.Optional});

            config.EnsureInitialized();
        }
    }
}