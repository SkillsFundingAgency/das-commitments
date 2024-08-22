using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler;

public class FluentValidationToApiErrorResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails? validationProblemDetails)
    {
        return new BadRequestObjectResultWithSubStatusSet(context.ModelState.CreateErrorResponse(), HttpSubStatusCode.DomainException);
    }
}

public class BadRequestObjectResultWithSubStatusSet : BadRequestObjectResult
{
    public BadRequestObjectResultWithSubStatusSet(object error, HttpSubStatusCode subStatusCode) : base(error)
    {
        SubStatusCode = subStatusCode;
    }

    public HttpSubStatusCode SubStatusCode { get; }

    public override void OnFormatting(ActionContext context)
    {
        base.OnFormatting(context);
        context.HttpContext.Response.SetSubStatusCode(SubStatusCode);
    }
}




