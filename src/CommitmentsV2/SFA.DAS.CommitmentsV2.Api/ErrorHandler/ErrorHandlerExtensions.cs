using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

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
                    if (contextFeature.Error is DomainException modelException)
                    {
                        context.Response.SetStatusCode(HttpStatusCode.BadRequest);
                        context.Response.SetSubStatusCode(HttpSubStatusCode.DomainException);
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

        public static string WriteErrorResponse(DomainException domainException)
        {
            var response = new ErrorResponse(MapToApiErrors(domainException.DomainErrors));
            return JsonConvert.SerializeObject(response);
        }

        private static List<ErrorDetail> MapToApiErrors(IEnumerable<DomainError> source)
        {
            return source.Select(sourceItem => new ErrorDetail(sourceItem.PropertyName, sourceItem.ErrorMessage)).ToList();
        }
    }
}