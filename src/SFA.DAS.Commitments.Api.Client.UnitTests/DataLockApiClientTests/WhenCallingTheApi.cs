using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Types.DataLock;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.DataLockApiClientTests
{
    public class WhenCallingTheApi
    {
        private Mock<ICommitmentsApiClientConfiguration> _configuration;
        private Mock<SecureHttpClient> _client;
        private DataLockApi _datalockApi;

        private const string ExpectedApiBaseUrl = "http://local.test";

        [SetUp]
        public void Arrange()
        {
            _configuration = new Mock<ICommitmentsApiClientConfiguration>();
            _configuration.Setup(x => x.BaseUrl).Returns(ExpectedApiBaseUrl);

            _client = new Mock<SecureHttpClient>();

            _datalockApi = new DataLockApi(_configuration.Object,_client.Object);
        }

        [Test]
        public async Task ThenTheCorrectUrlAndTypeIsReturnedForGetDataLockSingle()
        {
            //Arrange
            var expectedApprenticeshipId = 123125;
            var expectedDataLockEventId = 898565;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/apprenticeships/{expectedApprenticeshipId}/datalocks/{expectedDataLockEventId}";
            _client.Setup(x => x.GetAsync(expectedUrl)).ReturnsAsync(JsonConvert.SerializeObject(new DataLockStatus()));

            //Act
            var actual = await _datalockApi.GetDataLock(expectedApprenticeshipId, expectedDataLockEventId);

            //Assert
            Assert.IsAssignableFrom<DataLockStatus>(actual);
            _client.Verify(x => x.GetAsync(expectedUrl));
        }

        [Test]
        public async Task ThenTheCorrectUrlAndTypeIsReturnedForGetDataLockMultiple()
        {
            //Arrange
            var expectedApprenticeshipId = 123125;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/apprenticeships/{expectedApprenticeshipId}/datalocks";
            _client.Setup(x => x.GetAsync(expectedUrl)).ReturnsAsync(JsonConvert.SerializeObject(new List<DataLockStatus>()));

            //Act
            var actual = await _datalockApi.GetDataLocks(expectedApprenticeshipId);

            //Assert
            Assert.IsAssignableFrom<List<DataLockStatus>>(actual);
            _client.Verify(x => x.GetAsync(expectedUrl));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForPatchDataLock()
        {
            //Arrange
            var expectedApprenticeshipId = 123125;
            var expectedDataLockId = 435345;

            //Act
            await _datalockApi.PatchDataLock(expectedApprenticeshipId, new DataLockStatus {DataLockEventId = expectedDataLockId });

            //Assert
            _client.Verify(x => x.PatchAsync($"{ExpectedApiBaseUrl}/api/apprenticeships/{expectedApprenticeshipId}/datalocks/{expectedDataLockId}",It.IsAny<string>()));
        }
    }
}
