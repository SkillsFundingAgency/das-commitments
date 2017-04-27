using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.RelationshipApiClientTests
{
    public class WhenCallingTheApi
    {
        private Mock<ICommitmentsApiClientConfiguration> _configuration;
        private Mock<SecureHttpClient> _client;
        private RelationshipApi _relationshipApi;

        private const long ExpectedProviderId = 123123;
        private const string ExpectedApiBaseUrl = "http://local.test";

        [SetUp]
        public void Arrange()
        {
            _configuration = new Mock<ICommitmentsApiClientConfiguration>();
            _configuration.Setup(x => x.BaseUrl).Returns(ExpectedApiBaseUrl);

            _client = new Mock<SecureHttpClient>();

            _relationshipApi = new RelationshipApi(_configuration.Object,_client.Object);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetRelationshipAndTypeReturned()
        {
            //Arrange
            var expectedAccountId = 12355;
            var expectedLegalEntityID = "989856";
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/relationships/{expectedAccountId}/{expectedLegalEntityID}";
            _client.Setup(x => x.GetAsync(expectedUrl)).ReturnsAsync(JsonConvert.SerializeObject(new Relationship {EmployerAccountId = expectedAccountId}));

            //Act
            var actual = await _relationshipApi.GetRelationship(ExpectedProviderId, expectedAccountId, expectedLegalEntityID);

            //Assert
            Assert.IsAssignableFrom<Relationship>(actual);
            _client.Verify(x=>x.GetAsync(expectedUrl));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetRelationshipByCommitmentAndTypeReturned()
        {
            //Arrange
            var expectedCommitmentId = 989856;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/relationships/{expectedCommitmentId}";
            _client.Setup(x => x.GetAsync(expectedUrl)).ReturnsAsync(JsonConvert.SerializeObject(new Relationship { EmployerAccountId = 5436 }));

            //Act
            var actual = await _relationshipApi.GetRelationshipByCommitment(ExpectedProviderId, expectedCommitmentId);

            //Assert
            Assert.IsAssignableFrom<Relationship>(actual);
            _client.Verify(x => x.GetAsync(expectedUrl));
        }

        [Test]
        public async Task ThenTheCorrectURlIsCalledForPatchRelationship()
        {
            //Arrange
            var expectedAccountId = 12355;
            var expectedLegalEntityID = "989856";

            //Act
            await _relationshipApi.PatchRelationship(ExpectedProviderId, expectedAccountId, expectedLegalEntityID,new RelationshipRequest());

            //Assert
            _client.Verify(x => x.PatchAsync($"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/relationships/{expectedAccountId}/{expectedLegalEntityID}", It.IsAny<string>()));
        }
    }
}
