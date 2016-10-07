using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using FluentValidation;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api
{
    public class CustomExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is ValidationException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                var message = ((ValidationException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);
                return;
            }
            else if (context.Exception is UnauthorizedException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                var message = ((UnauthorizedException)context.Exception).Message;
                response.Content = new StringContent(message);
                context.Result = new CustomErrorResult(context.Request, response);
                return;
            }

            base.Handle(context);
        }
    }
}
