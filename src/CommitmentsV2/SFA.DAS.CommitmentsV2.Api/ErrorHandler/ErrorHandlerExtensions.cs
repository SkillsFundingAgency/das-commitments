using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler
{
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
                    if (contextFeature.Error is ApiException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var error = contextFeature.Error as ApiException;
                        logger.LogError($"Domain Error thrown: {contextFeature.Error}");
                        await context.Response.WriteAsync(WriteErrorDetails(error));
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                        await context.Response.WriteAsync(WriteErrorDetails(context.Response.StatusCode, "Internal Server Error."));
                    }
                }
            }

            app.UseExceptionHandler(appError =>
            {
                appError.Run(Handler);
            });
            return app;
        }

        public static string WriteErrorDetails(ApiException exception)
        {
            return JsonConvert.SerializeObject(new ErrorDetails {ErrorCode = exception.ErrorCode, Message = exception.Message});
        }

        public static string WriteErrorDetails(int statusCode, string message)
        {
            return JsonConvert.SerializeObject(new ErrorDetails { ErrorCode = statusCode, Message = message });
        }
    }
}
