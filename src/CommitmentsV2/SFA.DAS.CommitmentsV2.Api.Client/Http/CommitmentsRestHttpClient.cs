using System;
using System.Net.Http;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class CommitmentsRestHttpClient : RestHttpClient
    {
        public CommitmentsRestHttpClient(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {
            switch (httpResponseMessage.GetSubStatusCode())
            {
                case HttpSubStatusCode.None:
                    return base.CreateClientException(httpResponseMessage, content);
                case HttpSubStatusCode.DomainException:
                    return new CommitmentsApiModelException(JsonConvert.DeserializeObject<ErrorResponse>(content).Errors);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}