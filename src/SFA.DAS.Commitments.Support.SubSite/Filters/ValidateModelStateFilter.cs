using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;

namespace SFA.DAS.Commitments.Support.SubSite.Filters
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