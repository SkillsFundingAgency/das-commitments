using System;
using System.Net.Http;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class CustomRestHttpClient : RestHttpClient
    {
        public CustomRestHttpClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public override Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {

            var apiException = ConvertContentToApiException(httpResponseMessage, content);

            if (apiException != null)
            {
                return apiException;
            }

            return base.CreateClientException(httpResponseMessage, content);
        }

        private Exception ConvertContentToApiException(HttpResponseMessage httpResponseMessage, string content)
        {
            try
            {
                var errorDetails = JsonConvert.DeserializeObject<ErrorDetails>(content);

                if (errorDetails != null && errorDetails.Message != null)
                {
                    return new ApiException(errorDetails.ErrorCode, errorDetails.Message);
                }
            }
            catch (Exception)
            {
                // Do nothing 
            }

            return null;
        }



    }
}