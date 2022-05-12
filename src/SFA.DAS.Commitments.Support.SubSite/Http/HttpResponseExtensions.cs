using System.Net;
using Microsoft.AspNetCore.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Http;

namespace SFA.DAS.Commitments.Support.SubSite.Http
{
    public static class HttpResponseExtensions
    {
        public static void SetStatusCode(this HttpResponse httpResponse, HttpStatusCode httpStatusCode)
        {
            httpResponse.StatusCode = (int)httpStatusCode;
        }

        public static void SetSubStatusCode(this HttpResponse httpResponse, HttpSubStatusCode httpSubStatusCode)
        {
            httpResponse.Headers[HttpHeaderNames.SubStatusCode] = ((int)httpSubStatusCode).ToString();
        }
    }
}