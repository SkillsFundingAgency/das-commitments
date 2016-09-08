using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SFA.DAS.Tasks.Api
{
    public class ValidationErrorResult : IHttpActionResult
    {
        private HttpRequestMessage _request;
        private readonly HttpResponseMessage _httpResponseMessage;

        public ValidationErrorResult(HttpRequestMessage request, HttpResponseMessage httpResponseMessage)
        {
            _request = request;
            _httpResponseMessage = httpResponseMessage;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_httpResponseMessage);
        }
    }
}
