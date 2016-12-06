using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using FluentValidation;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api
{
    public class CustomExceptionHandler : ExceptionHandler
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is ValidationException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                var message = ((ValidationException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);

                _logger.Warn(context.Exception, "Validation error");

                return;
            }

            if (context.Exception is UnauthorizedException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                var message = ((UnauthorizedException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);

                _logger.Warn(context.Exception, "Authorisation error");

                return;
            }

            _logger.Error(context.Exception, "Unhandled exception");

            base.Handle(context);
        }
    }
}
