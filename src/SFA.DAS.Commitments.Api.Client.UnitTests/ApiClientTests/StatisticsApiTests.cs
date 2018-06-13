using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ApiClientTests
{
    [TestFixture]
    public class StatisticsApiTests
    {
        private FakeResponseHandler _fakeHandler;
        private StatisticsApi _statisticsApiClient;
        private const string ExpectedApiBaseUrl = "http://test.local.url/";

        [SetUp]
        public void Arrange()
        {
            _fakeHandler = new FakeResponseHandler();

            var httpClient = new HttpClient(_fakeHandler);

            var config = new Mock<ICommitmentsApiClientConfiguration>();
            config.Setup(m => m.BaseUrl).Returns(ExpectedApiBaseUrl);

            _statisticsApiClient = new StatisticsApi(httpClient, config.Object);
        }

        [Test]
        public async Task GetStatistics()
        {
            var statisticsRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + "api/statistics"), string.Empty );
            _fakeHandler.AddFakeResponse(statisticsRequest, new HttpResponseMessage(){StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty)});

            await _statisticsApiClient.GetStatistics();

            Assert.Pass();
        }

    }
}
