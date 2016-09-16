using System;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using SFA.DAS.Commitments.Api.ModelBinders;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var provider = new SimpleModelBinderProvider(typeof(CommitmentStatusChange), new CommitmentStatusChangeBinder());
            config.Services.Insert(typeof(ModelBinderProvider), 0, provider);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );
        }
    }
}
