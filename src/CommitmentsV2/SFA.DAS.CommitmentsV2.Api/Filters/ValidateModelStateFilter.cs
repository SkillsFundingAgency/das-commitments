using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;

namespace SFA.DAS.CommitmentsV2.Api.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.HttpContext.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                context.Result = new BadRequestObjectResult(context.ModelState.CreateErrorResponse());
            }
        }
    }
}
