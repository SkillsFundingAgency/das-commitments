using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;

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
                    if (contextFeature.Error is CommitmentsApiModelException modelException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        logger.LogError($"Model Error thrown: {modelException}");
                        await context.Response.WriteAsync(WriteErrorResponse(modelException));
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

        public static string WriteErrorResponse(CommitmentsApiModelException domainException)
        {
            var response = new ErrorResponse(domainException.Errors);
            return JsonConvert.SerializeObject(response);
        }
    }
}