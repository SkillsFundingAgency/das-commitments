using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Infrastructure.Api
{
    public class WhenCallingPost
    {

        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Accepted_And_Returns_Object(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var testBody = new TestBody { Content = "Any old thing" };
            var postApiRequest = new PostApiTestRequest { Data = testBody };
            var apiResponseObject = new TestResponse {PropA = "ABCD"};

            var apiResponse = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(apiResponseObject)),
                StatusCode = HttpStatusCode.OK
            };

            var httpMessageHandler = PostMessageHandler.SetupMessageHandlerMock(apiResponse, config.BaseUrl + postApiRequest.PostUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApprovalsOuterApiClient(client, config, Mock.Of<ILogger<ApprovalsOuterApiClient>>());

            //Act
            var response = await apiClient.PostWithResponseCode<TestBody, TestResponse>(postApiRequest);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Body.Should().BeOfType<TestResponse>().Which.PropA.Should().Be("ABCD");
        }

        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Accepted(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var testBody = new TestBody { Content = "Any old thing" };
            var postApiRequest = new PostApiTestRequest { Data = testBody };

            var apiResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };

            var httpMessageHandler = PostMessageHandler.SetupMessageHandlerMock(apiResponse, config.BaseUrl + postApiRequest.PostUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApprovalsOuterApiClient(client, config, Mock.Of<ILogger<ApprovalsOuterApiClient>>());

            //Act
            var response = await apiClient.PostWithResponseCode<TestBody, object>(postApiRequest, false);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Body.Should().BeNull();
        }

        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called_But_BadRequest_Is_Returned(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var testBody = new TestBody { Content = "Any old thing" };
            var postApiRequest = new PostApiTestRequest { Data = testBody };

            var apiResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };

            var httpMessageHandler = PostMessageHandler.SetupMessageHandlerMock(apiResponse, config.BaseUrl + postApiRequest.PostUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApprovalsOuterApiClient(client, config, Mock.Of<ILogger<ApprovalsOuterApiClient>>());

            //Act
            var response = await apiClient.PostWithResponseCode<TestBody, object>(postApiRequest, false);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Body.Should().BeNull();
        }

        [Test, AutoData]
        public async Task Then_If_It_Is_Not_Found_Default_Is_Returned(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var testBody = new TestBody { Content = "Hello" };
            var postApiRequest = new PostApiTestRequest { Data = testBody };
            
            var apiResponse = new HttpResponseMessage
            {
                Content = new StringContent("Error"),
                StatusCode = HttpStatusCode.NotFound
            };
            
            var httpMessageHandler = PostMessageHandler.SetupMessageHandlerMock(apiResponse, config.BaseUrl + postApiRequest.PostUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApprovalsOuterApiClient(client, config, Mock.Of<ILogger<ApprovalsOuterApiClient>>());
            
            //Act Assert
            var response = await apiClient.PostWithResponseCode<TestBody, object>(postApiRequest);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Body.Should().BeNull();

            response.ErrorContent.Should().Be("Error");
        }

        private class PostApiTestRequest : IPostApiRequest<TestBody>
        {
            public string PostUrl => "/api/post-endpoint";
            public TestBody Data { get; set; }
        }

        private class TestBody
        {
            public string Content { get; set; }
        }

        private class TestResponse
        {
            public string PropA { get; set; }
        }
    }

    public static class PostMessageHandler
    {
        public static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Post)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").First().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").First().Equals("1")
                        && c.RequestUri.AbsoluteUri.Equals(url)
                        ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
}