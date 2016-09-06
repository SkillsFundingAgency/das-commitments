using System;
using System.Web.Http;

namespace SFA.DAS.Tasks.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }
    }
}
