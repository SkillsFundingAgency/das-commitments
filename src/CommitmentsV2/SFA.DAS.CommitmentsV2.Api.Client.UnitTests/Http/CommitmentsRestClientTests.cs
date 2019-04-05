using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.CommitmentsV2.Api.Client.UnitTests.Fakes;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.Http;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.Client.UnitTests.Http
{
    [TestFixture]
    [Parallelizable]
    public class CommitmentsRestClientTests : FluentTest<CommitmentsRestClientTestsFixture>
    {
        [Test]
        public Task WhenCallingGetAndHttpClientReturnsNonSuccess_ThenShouldThrowRestClientException()
        {
            return TestExceptionAsync(f => f.SetupHttpClientGetToReturnInternalServerErrorWithStringResponseBody(), f => f.CallGet(null),
                (f, r) => r.Should().Throw<RestHttpClientException>()
                    .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError
                                 && ex.ReasonPhrase == "Internal Server Error"
                                 && Equals(ex.RequestUri, f.RequestUri)
                                 && ex.ErrorResponse.Contains(f.ResponseString)));
        }

        [Test]
        public Task WhenCallingPostAsJsonAndHttpClientReturnsBadRequestWithModelException_ThenShouldThrowApiModelException()
        {
            return TestExceptionAsync(f => f.SetupHttpClientGetToReturnModelError(),
                f => f.CallPostAsJson(null),
                (f, r) => r.Should().Throw<CommitmentsApiModelException>()
                    .Where(ex => ex.Errors.Count == f.ModelErrors.Count));
        }
    }

    public class CommitmentsRestClientTestsFixture
    {
        public class ExampleResponseObject
        {
            public string StringProperty { get; set; }
        }
        
        public FakeHttpMessageHandler HttpMessageHandler { get; set; }
        public HttpClient HttpClient { get; set; }
        public IRestHttpClient RestHttpClient { get; set; }
        public Uri RequestUri { get; set; }
        public string ResponseString { get; set; }
        public object ResponseObject { get; set; }
        public List<ErrorDetail> ModelErrors { get; set; }
        
        public CommitmentsRestClientTestsFixture()
        {
            ModelErrors = new List<ErrorDetail> { new ErrorDetail("field1", "Message1") };

            HttpMessageHandler = new FakeHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler) { BaseAddress = new Uri("https://example.com") };
            RestHttpClient = new CommitmentsRestHttpClient(HttpClient);
        }
        
        public void SetupHttpClientGetToReturnModelError()
        {
            ResponseObject = new ErrorResponse(ModelErrors);

            var stringBody = JsonConvert.SerializeObject(ResponseObject);
            HttpMessageHandler.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(stringBody, Encoding.Default, "application/json")
            };
        }

        public void SetupHttpClientGetToReturnInternalServerErrorWithStringResponseBody()
        {
            ResponseString = "Some sort of error description";
            RequestUri = new Uri($"{HttpClient.BaseAddress}/request", UriKind.Absolute);
            HttpMessageHandler.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ResponseString, Encoding.Default, "text/plain"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri)
            };
        }

        public async Task<string> CallGet(object queryData)
        {
            return await RestHttpClient.Get("https://example.com", queryData);
        }
        public async Task<string> CallPostAsJson(object queryData)
        {
            return await RestHttpClient.PostAsJson("https://example.com", queryData);
        }
    }
}