using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Models.Api;

namespace SFA.DAS.CommitmentsV2.UnitTests.Infrastructure.Api
{
    public class WhenCallingGet
    {
        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Returned(
            List<string> testObject, 
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var getTestRequest = new GetTestRequest();
            
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(testObject)),
                StatusCode = HttpStatusCode.Accepted
            };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, config.BaseUrl + getTestRequest.GetUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApiClient(client, config);

            //Act
            var actual = await apiClient.Get<List<string>>(getTestRequest);
            
            //Assert
            actual.Should().BeEquivalentTo(testObject);
        }
        
        [Test, AutoData]
        public void Then_If_It_Is_Not_Successful_An_Exception_Is_Thrown(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var getTestRequest = new GetTestRequest();
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };
            
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, config.BaseUrl + getTestRequest.GetUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApiClient(client, config);
            
            //Act Assert
            Assert.ThrowsAsync<HttpRequestException>(() => apiClient.Get<List<string>>(getTestRequest));
            
        }
        
        [Test, AutoData]
        public async Task Then_If_It_Is_Not_Found_Default_Is_Returned(
            ApprovalsOuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = "https://test.local";
            var getTestRequest = new GetTestRequest();
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.NotFound
            };
            
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, config.BaseUrl + getTestRequest.GetUrl, config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new ApiClient(client, config);
            
            //Act Assert
            var actual = await apiClient.Get<List<string>>(getTestRequest);

            actual.Should().BeNull();

        }

        private class GetTestRequest : IGetApiRequest
        {
            public string GetUrl => "/test-url/get";
        }
    }
    public static class MessageHandler
    {
        public static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").First().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").First().Equals("1")
                        && c.RequestUri.AbsoluteUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
}