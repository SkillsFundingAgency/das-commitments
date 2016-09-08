using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace SFA.DAS.Tasks.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Services.Replace(typeof (IExceptionHandler), new ValidationExceptionHandler());
        }
    }
}
