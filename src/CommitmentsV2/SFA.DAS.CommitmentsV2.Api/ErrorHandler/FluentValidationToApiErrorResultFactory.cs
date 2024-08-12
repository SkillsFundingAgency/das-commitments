using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler;

public class FluentValidationToApiErrorResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails? validationProblemDetails)
    {
        return new BadRequestObjectResult(context.ModelState.CreateErrorResponse());
    }
}