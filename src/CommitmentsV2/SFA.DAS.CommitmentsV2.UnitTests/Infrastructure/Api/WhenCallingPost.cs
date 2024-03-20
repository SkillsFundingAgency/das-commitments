using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Infrastructure.Api
{
    public class WhenCallingPost
    {
        [Test, MoqAutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Posted(
            StopApprenticeshipRequestRequest postTestRequest,
            Mock<ILogger<ApprovalsOuterApiClient>> logger)
        {
            // Arrange
            const string key = "123-abc-567";

            var config = new ApprovalsOuterApiConfiguration { BaseUrl = "http://valid-url/", Key = key };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty)
            };

            var httpMessageHandler = SetupMessageHandlerMock(response, $"{config.BaseUrl}{postTestRequest.PostUrl}", config.Key);
            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = new Uri(config.BaseUrl) };

            var apiClient = new ApprovalsOuterApiClient(httpClient, config, logger.Object);

            // Act
            await apiClient.PostAsync<string>(postTestRequest);

            // Assert
            httpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(c =>
                    c.Method.Equals(HttpMethod.Post)
                    && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                    && c.Headers.GetValues("Ocp-Apim-Subscription-Key").First().Equals(key)
                    && c.Headers.Contains("X-Version")
                    && c.Headers.GetValues("X-Version").First().Equals("1")
                    && c.RequestUri.AbsoluteUri.Equals($"{config.BaseUrl}{postTestRequest.PostUrl}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test, MoqAutoData]
        public async Task Then_If_It_Is_Not_Successful_An_Exception_Is_ThrownAsync(
            StopApprenticeshipRequestRequest postTestRequest,
            Mock<ILogger<ApprovalsOuterApiClient>> logger)
        {
            // Arrange
            const string key = "123-abc-567";

            var config = new ApprovalsOuterApiConfiguration { BaseUrl = "http://valid-url/", Key = key };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var httpMessageHandler = SetupMessageHandlerMock(response, $"{config.BaseUrl}{postTestRequest.PostUrl}", config.Key);
            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = new Uri(config.BaseUrl) };

            var apiClient = new ApprovalsOuterApiClient(httpClient, config, logger.Object);

            await apiClient.Invoking(async x => await x.PostAsync<string>(postTestRequest)).Should().ThrowAsync<HttpRequestException>();
        }

        private static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
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
                        && c.RequestUri.AbsoluteUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            return httpMessageHandler;
        }
    }
}

