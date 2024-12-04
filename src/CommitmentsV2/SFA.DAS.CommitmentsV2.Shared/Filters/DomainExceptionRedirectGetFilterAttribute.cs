using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.Validation.Mvc.Extensions;

namespace SFA.DAS.CommitmentsV2.Shared.Filters;

public class DomainExceptionRedirectGetFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not CommitmentsApiModelException exception)
        {
            return;
        }

        var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = tempDataFactory.GetTempData(context.HttpContext);

        var modelState = context.ModelState;
        modelState.AddModelExceptionErrors(exception);
        var serializableModelState = modelState.ToSerializable();

        tempData.Set(serializableModelState);

        context.RouteData.Values.Merge(context.HttpContext.Request.Query);
        context.Result = new RedirectToRouteResult(context.RouteData.Values);
    }
}