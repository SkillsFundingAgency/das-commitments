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
            if (contextFeature != null)
            {
                var exception = contextFeature.Error;
                if (exception is TargetInvocationException || exception is AggregateException)
                    exception = exception.InnerException;

                if (exception is DomainException modelException)
                {
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                    logger.LogError("Model Error thrown: {modelException}", modelException);
                    await context.Response.WriteAsync(WriteErrorResponse(modelException));
                }
                if (exception is BulkUploadDomainException bulkUploadDomainException)
                {
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.BulkUploadDomainException);
                    logger.LogError("Model Error thrown: {bulkUploadDomainException}", bulkUploadDomainException);
                    await context.Response.WriteAsync(WriteErrorResponse(bulkUploadDomainException));
                }
                if (exception is ValidationException validationException)
                {
                    context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                    context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
                    logger.LogError("Command/Query Validation Error thrown: {validationException}", validationException);
                    await context.Response.WriteAsync(WriteErrorResponse(validationException));
                }
                else
                {
                    logger.LogError("Something went wrong: {contextFeatureError}", exception);
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