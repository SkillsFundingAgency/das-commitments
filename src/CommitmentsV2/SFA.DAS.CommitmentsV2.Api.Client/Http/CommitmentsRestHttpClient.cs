using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class CommitmentsRestHttpClient : RestHttpClient
    {
        private readonly ILogger<CommitmentsRestHttpClient> _logger;

        public CommitmentsRestHttpClient(HttpClient httpClient, ILoggerFactory loggerFactory) : base(httpClient)
        {
            _logger = loggerFactory.CreateLogger<CommitmentsRestHttpClient>();
        }

        protected override Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {
            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest && httpResponseMessage.GetSubStatusCode() == HttpSubStatusCode.DomainException)
            {
                return CreateApiModelException(httpResponseMessage, content);
            }
            else if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest && httpResponseMessage.GetSubStatusCode() == HttpSubStatusCode.BulkUploadDomainException)
            {
                return CreateBulkUploadApiModelException(httpResponseMessage, content);
            }
            else
            {
               return base.CreateClientException(httpResponseMessage, content);
            }
        }

        private Exception CreateApiModelException(HttpResponseMessage httpResponseMessage, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning($"{httpResponseMessage.RequestMessage.RequestUri} has returned an empty string when an array of error responses was expected.");
                return new CommitmentsApiModelException(new List<ErrorDetail>());
            }

            var errors = new CommitmentsApiModelException(JsonConvert.DeserializeObject<ErrorResponse>(content).Errors);

            var errorDetails = string.Join(";", errors.Errors.Select(e => $"{e.Field} ({e.Message})"));
            _logger.Log(errors.Errors.Count == 0 ? LogLevel.Warning : LogLevel.Debug, $"{httpResponseMessage.RequestMessage.RequestUri} has returned {errors.Errors.Count} errors: {errorDetails}");

            return errors;
        }

        private Exception CreateBulkUploadApiModelException(HttpResponseMessage httpResponseMessage, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning($"{httpResponseMessage.RequestMessage.RequestUri} has returned an empty string when an array of error responses was expected.");
                return new CommitmentsApiBulkUploadModelException(new List<BulkUploadValidationError>());
            }

            var errors = new CommitmentsApiBulkUploadModelException(JsonConvert.DeserializeObject<BulkUploadErrorResponse>(content).DomainErrors?.ToList());

            return errors;
        }
    }
}