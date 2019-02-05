using System;
using System.Linq;
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
            var apiException = ConvertContentToApiException(content);

            if (apiException != null)
            {
                return apiException;
            }

            return base.CreateClientException(httpResponseMessage, content);
        }

        private Exception ConvertContentToApiException(string content)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(content);

                if (errorResponse != null)
                {
                    if (errorResponse.ErrorType == ErrorType.CommitmentApiException)
                    {
                        var errorDetail = errorResponse.ErrorDetails?.FirstOrDefault();
                        if (errorDetail != null)
                        {
                            return new CommitmentsApiException(errorDetail.ErrorCode, errorDetail.Message);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Consume the exception and do nothing
            }

            return null;
        }
    }
}