using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Authorization.Features.Configuration;
using System;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Authorization
{
    public class FeatureToggleAttribute : ActionFilterAttribute
    {
        public string Feature { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var config = filterContext?.HttpContext?.RequestServices?.GetService<FeaturesConfiguration>();
            
            var featureToggle = config?.FeatureToggles?.FirstOrDefault(s => s.Feature.Equals(Feature, StringComparison.InvariantCultureIgnoreCase));

            if(featureToggle != null && !featureToggle.IsEnabled)
            {
                filterContext.Result = new NotFoundResult();
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
