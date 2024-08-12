using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler;



public class CustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails? validationProblemDetails)
    {
        return new BadRequestObjectResult(new { Title = "Validation errors", ValidationErrors = validationProblemDetails?.Errors });
    }
}
public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseApiGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        async Task Handler(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                if (contextFeature.Error is DomainException modelException)
                {
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                    logger.LogError("Model Error thrown: {modelException}", modelException);
                    await context.Response.WriteAsync(WriteErrorResponse(modelException));
                }
                if (contextFeature.Error is BulkUploadDomainException bulkUploadDomainException)
                {
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.BulkUploadDomainException);
                    logger.LogError("Model Error thrown: {bulkUploadDomainException}", bulkUploadDomainException);
                    await context.Response.WriteAsync(WriteErrorResponse(bulkUploadDomainException));
                }
                else
                {
                    logger.LogError("Something went wrong: {contextFeatureError}", contextFeature.Error);
                }
            }
        }

        app.UseExceptionHandler(appError =>
        {
            appError.Run(Handler);
        });
        
        return app;
    }

    private static string WriteErrorResponse(DomainException domainException)
    {
        var response = new ErrorResponse(MapToApiErrors(domainException.DomainErrors));
        return JsonConvert.SerializeObject(response);
    }

    private static string WriteErrorResponse(BulkUploadDomainException domainException)
    {
        var response = new BulkUploadErrorResponse(domainException.DomainErrors);
        var responseAsString = JsonConvert.SerializeObject(response);
        return responseAsString;
    }

    private static List<ErrorDetail> MapToApiErrors(IEnumerable<DomainError> source)
    {
        return source.Select(sourceItem => new ErrorDetail(sourceItem.PropertyName, sourceItem.ErrorMessage)).ToList();
    }
}