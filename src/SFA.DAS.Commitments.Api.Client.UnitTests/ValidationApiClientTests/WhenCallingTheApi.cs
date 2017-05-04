using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Types.Validation;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ValidationApiClientTests
{
    public class WhenCallingTheApi
    {
        private Mock<ICommitmentsApiClientConfiguration> _configuration;
        private Mock<SecureHttpClient> _client;
        private ValidationApi _validationApi;

        private const string ExpectedApiBaseUrl = "http://local.test";

        [SetUp]
        public void Arrange()
        {
            _configuration = new Mock<ICommitmentsApiClientConfiguration>();
            _configuration.Setup(x => x.BaseUrl).Returns(ExpectedApiBaseUrl);

            _client = new Mock<SecureHttpClient>();

            _validationApi = new ValidationApi(_configuration.Object, _client.Object);
        }

        [Test]
        public async Task ThenTheCorrectUrlAndTypeIsReturnedForValidateOverlappingSingle()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/validation/apprenticeships/overlapping";
            var returnList = new List<ApprenticeshipOverlapValidationResult> {new ApprenticeshipOverlapValidationResult()};
            _client.Setup(x => x.PostAsync(expectedUrl, It.IsAny<string>())).ReturnsAsync(JsonConvert.SerializeObject(returnList));

            //Act
            var actual = await _validationApi.ValidateOverlapping(new ApprenticeshipOverlapValidationRequest());

            //Assert
            Assert.IsAssignableFrom<ApprenticeshipOverlapValidationResult>(actual);
            _client.Verify(x=>x.PostAsync(expectedUrl,It.IsAny<string>()));
        }


        [Test]
        public async Task ThenTheCorrectUrlAndTypeIsReturnedForValidateOverlappingMultiple()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/validation/apprenticeships/overlapping";
            var returnList = new List<ApprenticeshipOverlapValidationResult> { new ApprenticeshipOverlapValidationResult() };
            _client.Setup(x => x.PostAsync(expectedUrl, It.IsAny<string>())).ReturnsAsync(JsonConvert.SerializeObject(returnList));

            //Act
            var actual = await _validationApi.ValidateOverlapping(new List<ApprenticeshipOverlapValidationRequest>());

            //Assert
            Assert.IsAssignableFrom<List<ApprenticeshipOverlapValidationResult>>(actual);
            _client.Verify(x => x.PostAsync(expectedUrl, It.IsAny<string>()));
        }
    }
}
