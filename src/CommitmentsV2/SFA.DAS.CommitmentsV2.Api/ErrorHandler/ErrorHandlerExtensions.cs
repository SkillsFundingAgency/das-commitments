using System.Collections.Generic;
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
                    if (contextFeature.Error is CommitmentsApiException exception)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        logger.LogError($"Domain Error thrown: {exception}");
                        await context.Response.WriteAsync(WriteErrorResponse(exception));
                    }
                    else
                    {
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                    }
                }
            }

            app.UseExceptionHandler(appError =>
            {
                appError.Run(Handler);
            });
            return app;
        }

        public static string WriteErrorResponse(CommitmentsApiException exception)
        {
            var response = new ErrorResponse
            {
                ErrorType = ErrorType.CommitmentApiException,
                ErrorDetails = new List<ErrorDetail>{new ErrorDetail {ErrorCode = exception.ErrorCode, Message = exception.Message}}
            };

            return JsonConvert.SerializeObject(response);
        }
    }
}
