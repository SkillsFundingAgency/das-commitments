using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Http;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class HttpHelperTests
    {
        private const string Url = "http://url";

        [Test]
        public async Task HttpHelper_GetAsync_IsCalledCorrectly()
        {
            var testData = new object();
            var fixture = new HttpHelperTestsFixture();

            await fixture.Sut.GetAsync<object>(Url, testData, CancellationToken.None);

            fixture.MockRestHtpClient.Verify(x=>x.Get<object>(Url, testData, CancellationToken.None));
        }

        [Test]
        public async Task HttpHelper_GetAsync_PassesApiResultBackToCaller()
        {
            var testData = new object();
            var fixture = new HttpHelperTestsFixture();

            var result = await fixture.Sut.GetAsync<object>(Url, testData, CancellationToken.None);

            Assert.AreEqual(fixture.ApiResult, result);
        }

        [Test]
        public async Task HttpHelper_PostAsync_IsCalledCorrectly()
        {
            var testData = new object();
            var fixture = new HttpHelperTestsFixture();

            await fixture.Sut.PostAsJson<object, object>(Url, testData, CancellationToken.None);

            fixture.MockRestHtpClient.Verify(x => x.PostAsJson<object, object>(Url, testData, CancellationToken.None));
        }

        [Test]
        public async Task HttpHelper_PostAsync_PassesApiResultBackToCaller()
        {
            var testData = new object();
            var fixture = new HttpHelperTestsFixture();

            var result = await fixture.Sut.PostAsJson<object, object>(Url, testData, CancellationToken.None);

            Assert.AreEqual(fixture.ApiResult, result);
        }
    }

    public class HttpHelperTestsFixture
    {
        public object ApiResult;
        public HttpHelper Sut;
        public Mock<IRestHttpClient> MockRestHtpClient;
        public HttpHelperTestsFixture()
        {
            ApiResult = new object(); 
            MockRestHtpClient = new Mock<IRestHttpClient>();
            Sut = new HttpHelper(MockRestHtpClient.Object);

            MockRestHtpClient.Setup(x => x.Get<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResult);

            MockRestHtpClient.Setup(x => x.PostAsJson<object, object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiResult);

        }
    }
}
