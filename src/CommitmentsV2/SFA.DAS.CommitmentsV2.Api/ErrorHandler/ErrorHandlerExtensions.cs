using System.Net;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler;

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseApiGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        async Task Handler(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature == null)
            {
                return;
            }

            var exception = contextFeature.Error;
            if (exception is TargetInvocationException || exception is AggregateException)
                exception = exception.InnerException;
            var traceId = context.TraceIdentifier;

            switch (exception)
            {
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    logger.LogWarning(exception, "Unauthorized access. TraceId={TraceId}", traceId);
                    return;
                case DomainException modelException:
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                    logger.LogError(exception, "Model Error thrown. TraceId={TraceId}", traceId);
                    await context.Response.WriteAsync(WriteErrorResponse(modelException));
                    return;
                case BulkUploadDomainException bulkUploadDomainException:
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.BulkUploadDomainException);
                    logger.LogError(exception, "Bulk upload domain error. TraceId={TraceId}", traceId);
                    await context.Response.WriteAsync(WriteErrorResponse(bulkUploadDomainException));
                    return;
                case ValidationException validationException:
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                    logger.LogError(exception, "Command/Query Validation Error. TraceId={TraceId}", traceId);
                    await context.Response.WriteAsync(WriteErrorResponse(validationException));
                    return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            logger.LogError(exception,
                "Unhandled exception in pipeline. ExceptionType={ExceptionType} Message={Message} TraceId={TraceId} Path={Path}",
                exception.GetType().FullName,
                exception.Message,
                traceId,
                context.Request.Path.Value);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "An unexpected error occurred.", traceId }));
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

    private static string WriteErrorResponse(ValidationException validationException)
    {
        var response = new ErrorResponse(MapToApiErrors(validationException.Errors));
        return JsonConvert.SerializeObject(response);
    }

    private static List<ErrorDetail> MapToApiErrors(IEnumerable<ValidationFailure> source)
    {
        return source.Select(sourceItem => new ErrorDetail(sourceItem.PropertyName, sourceItem.ErrorMessage)).ToList();
    }

    private static List<ErrorDetail> MapToApiErrors(IEnumerable<DomainError> source)
    {
        return source.Select(sourceItem => new ErrorDetail(sourceItem.PropertyName, sourceItem.ErrorMessage)).ToList();
    }
}