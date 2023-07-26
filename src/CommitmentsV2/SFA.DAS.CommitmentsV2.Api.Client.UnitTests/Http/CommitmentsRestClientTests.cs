using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.CommitmentsV2.Api.Client.UnitTests.Fakes;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.Http;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.Api.Client.UnitTests.Http
{
    [TestFixture]
    [Parallelizable]
    public class CommitmentsRestClientTests : FluentTest<CommitmentsRestClientTestsFixture>
    {
        [Test]
        public Task CreateClientException_WhenResponseStatusIsBadRequestAndResponseSubStatusIsDomainException_ThenShouldThrowApiModelException()
        {
            return TestExceptionAsync(
                f => f.SetBadRequestResponseStatus().WithError("field1", "error1").SetDomainExceptionResponseSubStatus(),
                f => f.Get(),
                (f, r) => r.Should().ThrowAsync<CommitmentsApiModelException>().Where(ex =>
                    ex.Errors.Count == f.ModelErrors.Count));
        }

        [Test]
        public Task CreateClientException_WhenDomainExceptionIsDetectedAndDebugLoggingEnabled_ThenShouldLogErrorInformation()
        {
            return TestExceptionAsync(
                f => f
                    .WithError("field1", "error1")
                    .WithError("field2", "error2")
                    .SetBadRequestResponseStatus()
                    .SetDomainExceptionResponseSubStatus()
                    .SetLoggingLevel(LogLevel.Debug),
                f => f.Get(),
                (f, r) =>
                {
                    r.Should().ThrowAsync<CommitmentsApiModelException>();
                    f.FakeLogger.ContainsMessage(m => m.LogLevel == LogLevel.Debug && m.Message.Contains("field1") && m.Message.Contains("error1"));
                    f.FakeLogger.ContainsMessage(m => m.LogLevel == LogLevel.Debug && m.Message.Contains("field2") && m.Message.Contains("error2"));
                });
        }

        [Test]
        public Task CreateClientException_WhenDomainExceptionIsDetectedAndDebugLoggingIsNotEnabled_ThenShouldNotLogErrorInformation()
        {
            return TestExceptionAsync(
                f => f
                    .WithError("field1", "error1")
                    .WithError("field2", "error2")
                    .SetBadRequestResponseStatus()
                    .SetDomainExceptionResponseSubStatus(),
                f => f.Get(),
                (f, r) =>
                {
                    r.Should().ThrowAsync<CommitmentsApiModelException>();
                    Assert.AreEqual(0, f.FakeLogger.Messages.Count);
                });
        }

        [Test]
        public Task CreateClientException_WhenDomainExceptionIsDetectedAndWarningLoggingEnabled_ThenShouldLogWarningIfResponseIsEmpty()
        {
            return TestExceptionAsync(
                f => f
                    .SetBadRequestResponseStatus()
                    .SetDomainExceptionResponseSubStatus()
                    .SetLoggingLevel(LogLevel.Debug),
                f => f.Get(),
                (f, r) =>
                {
                    r.Should().ThrowAsync<CommitmentsApiModelException>();
                    f.FakeLogger.ContainsMessage(m => m.LogLevel == LogLevel.Warning && m.Message.Contains("has returned an empty string when an array of error responses was expected"));
                });
        }

        [Test]
        public Task CreateClientException_WhenDomainExceptionIsDetectedAndWarningLoggingIsNotEnabled_ThenShouldNotLogWarningIfResponseIsEmpty()
        {
            return TestExceptionAsync(
                f => f
                    .SetBadRequestResponseStatus()
                    .SetDomainExceptionResponseSubStatus(),
                f => f.Get(),
                (f, r) =>
                {
                    r.Should().ThrowAsync<CommitmentsApiModelException>();
                    Assert.AreEqual(0, f.FakeLogger.Messages.Count);
                });
        }

        [Test]
        public Task CreateClientException_WhenResponseStatusIsBadRequestAndResponseSubStatusIsNone_ThenShouldThrowRestHttpClientException()
        {
            return TestExceptionAsync(
                f => f.SetBadRequestResponseStatus(),
                f => f.Get(),
                (f, r) => r.Should().ThrowAsync<RestHttpClientException>().Where(ex => 
                    ex.StatusCode == HttpStatusCode.BadRequest &&
                    ex.ReasonPhrase == "Bad Request" &&
                    ex.RequestUri == f.RequestUri &&
                    ex.ErrorResponse == f.ResponseString));
        }
        
        [Test]
        public Task CreateClientException_WhenResponseStatusIsInternalServerError_ThenShouldThrowRestHttpClientException()
        {
            return TestExceptionAsync(
                f => f.SetInternalServerErrorResponseStatus(),
                f => f.Get(),
                (f, r) => r.Should().ThrowAsync<RestHttpClientException>().Where(ex => 
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
        public string ResponseString { get; set; } = string.Empty;
        public List<ErrorDetail> ModelErrors { get; set; }
        public Mock<ILoggerFactory> LoggerFactoryMock { get; set; }
        public FakeLogger FakeLogger { get; set; }
        
        public CommitmentsRestClientTestsFixture()
        {
            LoggerFactoryMock = new Mock<ILoggerFactory>();
            FakeLogger = new FakeLogger(); 
            LoggerFactoryMock
                    .Setup(lfm => lfm.CreateLogger(It.Is<string>(s => s.EndsWith(nameof(CommitmentsRestHttpClient)))))
                    .Returns(FakeLogger);

            ModelErrors = new List<ErrorDetail>();
            HttpMessageHandler = new FakeHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler) { BaseAddress = new Uri("https://example.com") };
            RestHttpClient = new CommitmentsRestHttpClient(HttpClient, LoggerFactoryMock.Object);
        }

        public CommitmentsRestClientTestsFixture SetLoggingLevel(LogLevel logLevel)
        {
            FakeLogger.EnableLevel(logLevel);
            return this;
        }

        public CommitmentsRestClientTestsFixture SetBadRequestResponseStatus()
        {
            RequestUri = new Uri($"{HttpClient.BaseAddress}/request", UriKind.Absolute);
            
            HttpMessageHandler.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri),
                Content = new StringContent(ResponseString, Encoding.Default, "text/plain")
            };

            return this;
        }

        public CommitmentsRestClientTestsFixture SetResponseStringFromModelErrors()
        {
            ResponseObject = new ErrorResponse(ModelErrors);
            ResponseString = JsonConvert.SerializeObject(ResponseObject);

            return this;
        }

        public CommitmentsRestClientTestsFixture SetDomainExceptionResponseSubStatus()
        {
            if (ModelErrors.Count > 0)
            {
                SetResponseStringFromModelErrors();
            }
            else
            {
                ResponseString = String.Empty;
            }

            HttpMessageHandler.HttpResponseMessage.Headers.Add(HttpHeaderNames.SubStatusCode, ((int)HttpSubStatusCode.DomainException).ToString());
            HttpMessageHandler.HttpResponseMessage.Content = new StringContent(ResponseString, Encoding.Default, "application/json");

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

        public CommitmentsRestClientTestsFixture WithError(string field, string message)
        {
            ModelErrors.Add(new ErrorDetail(field, message));
            return this;
        }

        public Task<string> Get()
        {
            return RestHttpClient.Get("https://example.com");
        }
    }
}