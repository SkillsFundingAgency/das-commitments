using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using FluentValidation;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Api
{
    public class CustomExceptionHandler : ExceptionHandler
    {
        private static readonly ILog Logger = new NLogLogger();

        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is ValidationException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                var message = ((ValidationException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);

                Logger.Warn(context.Exception, "Validation error");

                return;
            }

            if (context.Exception is UnauthorizedException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                var message = ((UnauthorizedException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);

                Logger.Warn(context.Exception, "Authorisation error");

                return;
            }

            if (context.Exception is ResourceNotFoundException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                var message = ((ResourceNotFoundException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);

                Logger.Warn(context.Exception, "Unable to locate resource error");

                return;
            }

            Logger.Error(context.Exception, "Unhandled exception");

            base.Handle(context);
        }
    }
}
