using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class RestHttpClientException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public Uri RequestUri { get; }
        public string ErrorResponse { get; }

        public RestHttpClientException(HttpResponseMessage httpResponseMessage, string errorResponse)
            : base(GenerateMessage(httpResponseMessage, errorResponse))
        {
            StatusCode = httpResponseMessage.StatusCode;
            ReasonPhrase = httpResponseMessage.ReasonPhrase;
            RequestUri = httpResponseMessage.RequestMessage.RequestUri;
            ErrorResponse = errorResponse;
        }
        
        // assumes response content hasn't already been read
        public static async Task<RestHttpClientException> Create(HttpResponseMessage httpResponseMessage)
        {
            var ErrorDetails = await httpResponseMessage.Content.ReadAsAsync<ErrorDetails>();

            if (ErrorDetails != null && ErrorDetails.Message != null)
            {

            }

            return new RestHttpClientException(httpResponseMessage,
                    await httpResponseMessage.Content.ReadAsStringAsync());
        }

        private static string GenerateMessage(HttpResponseMessage httpResponseMessage, string errorResponse)
        {
            return
$@"Request '{httpResponseMessage.RequestMessage.RequestUri}' returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}
Response:
{errorResponse}";
        }
    }
}