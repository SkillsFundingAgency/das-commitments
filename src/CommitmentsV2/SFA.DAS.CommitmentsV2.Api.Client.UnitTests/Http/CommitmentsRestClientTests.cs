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
using SFA.DAS.CommitmentsV2.Api.Types.Http;
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
        public Task
            CreateClientException_WhenResponseStatusIsBadRequestAndResponseSubStatusIsDomainException_ThenShouldThrowApiModelException()
        {
            return TestExceptionAsync(
                f => f.SetBadRequestResponseStatus().SetDomainExceptionResponseSubStatus(),
                f => f.Get(),
                (f, r) => r.Should().Throw<CommitmentsApiModelException>().Where(ex =>
                    ex.Errors.Count == f.ModelErrors.Count));
        }

        [Test]
        public Task
            CreateClientException_WhenResponseStatusIsBadRequestAndResponseSubStatusIsNone_ThenShouldThrowRestHttpClientException()
        {
            return TestExceptionAsync(
                f => f.SetBadRequestResponseStatus(),
                f => f.Get(),
                (f, r) => r.Should().Throw<RestHttpClientException>().Where(ex =>
                    ex.StatusCode == HttpStatusCode.BadRequest &&
                    ex.ReasonPhrase == "Bad Request" &&
                    ex.RequestUri == f.RequestUri &&
                    ex.ErrorResponse == f.ResponseString));
        }

        [Test]
        public Task
            CreateClientException_WhenResponseStatusIsInternalServerError_ThenShouldThrowRestHttpClientException()
        {
            return TestExceptionAsync(
                f => f.SetInternalServerErrorResponseStatus(),
                f => f.Get(),
                (f, r) => r.Should().Throw<RestHttpClientException>().Where(ex =>
                    ex.StatusCode == HttpStatusCode.InternalServerError &&
                    ex.ReasonPhrase == "Internal Server Error" &&
                    ex.RequestUri == f.RequestUri &&
                    ex.ErrorResponse == f.ResponseString));
        }
    }

    public class CommitmentsRestClientTestsFixture
    {
        public FakeHttpMessageHandler HttpMessageHandler { get; set; }
        public HttpClient HttpClient { get; set; }
        public IRestHttpClient RestHttpClient { get; set; }
        public Uri RequestUri { get; set; }
        public object ResponseObject { get; set; }
        public string ResponseString { get; set; }
        public List<ErrorDetail> ModelErrors { get; set; }

        public CommitmentsRestClientTestsFixture()
        {
            ModelErrors = new List<ErrorDetail> {new ErrorDetail("Field1", "Message1")};
            HttpMessageHandler = new FakeHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler) {BaseAddress = new Uri("https://example.com")};
            RestHttpClient = new CommitmentsRestHttpClient(HttpClient);
        }

        public CommitmentsRestClientTestsFixture SetBadRequestResponseStatus()
        {
            RequestUri = new Uri($"{HttpClient.BaseAddress}/request", UriKind.Absolute);
            ResponseString = "Foobar";

            HttpMessageHandler.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri),
                Content = new StringContent(ResponseString, Encoding.Default, "text/plain")
            };

            return this;
        }

        public CommitmentsRestClientTestsFixture SetDomainExceptionResponseSubStatus()
        {
            ResponseObject = new ErrorResponse(ModelErrors);
            ResponseString = JsonConvert.SerializeObject(ResponseObject);
            HttpMessageHandler.HttpResponseMessage.Headers.Add(HttpHeaderNames.SubStatusCode,
                ((int) HttpSubStatusCode.DomainException).ToString());
            HttpMessageHandler.HttpResponseMessage.Content =
                new StringContent(ResponseString, Encoding.Default, "application/json");

            return this;
        }

        public CommitmentsRestClientTestsFixture SetInternalServerErrorResponseStatus()
        {
            RequestUri = new Uri($"{HttpClient.BaseAddress}/request", UriKind.Absolute);
            ResponseString = "Foobar";

            HttpMessageHandler.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri),
                Content = new StringContent(ResponseString, Encoding.Default, "text/plain")
            };

            return this;
        }

        public Task<string> Get()
        {
            return RestHttpClient.Get("https://example.com");
        }
    }
}