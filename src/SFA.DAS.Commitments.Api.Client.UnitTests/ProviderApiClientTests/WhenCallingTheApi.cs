using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ProviderApiClientTests
{
    public class WhenCallingTheApi
    {
        private Mock<IHttpCommitmentHelper> _commitmentHelper;
        private Mock<ICommitmentsApiClientConfiguration> _configuration;
        private ProviderCommitmentsApi _providerCommitmentsApi;

        private const long ExpectedProviderId = 763716;
        private const string ExpectedApiBaseUrl = "http://local.test";

        [SetUp]
        public void Arrange()
        {
            _commitmentHelper = new Mock<IHttpCommitmentHelper>();
            _configuration = new Mock<ICommitmentsApiClientConfiguration>();
            _configuration.Setup(x => x.BaseUrl).Returns(ExpectedApiBaseUrl);

            _providerCommitmentsApi = new ProviderCommitmentsApi(_configuration.Object, _commitmentHelper.Object);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingPatchProviderCommitment()
        {
            //Arrange
            var expectedCommitmentId = 324236;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}";

            //Act
            await _providerCommitmentsApi.PatchProviderCommitment(ExpectedProviderId, expectedCommitmentId, new CommitmentSubmission());

            //Assert
            _commitmentHelper.Verify(x=>x.PatchCommitment(expectedUrl, It.IsAny<CommitmentSubmission>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingGetProviderApprenticeshipsAndTheCorrectTypeReturned()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/";
            _commitmentHelper.Setup(x => x.GetApprenticeships(expectedUrl)).ReturnsAsync(new List<Apprenticeship>());

            //Act
            var actual = await _providerCommitmentsApi.GetProviderApprenticeships(ExpectedProviderId);

            //Assert
            _commitmentHelper.Verify(x => x.GetApprenticeships(expectedUrl));
            Assert.IsAssignableFrom<List<Apprenticeship>>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingGetProviderApprenticeshipAndTheCorrectTypeReturned()
        {
            //Arrange
            var expectedApprenticeshipId = 2349028;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/{expectedApprenticeshipId}";
            _commitmentHelper.Setup(x => x.GetApprenticeship(expectedUrl)).ReturnsAsync(new Apprenticeship());

            //Act
            var actual = await _providerCommitmentsApi.GetProviderApprenticeship(ExpectedProviderId, expectedApprenticeshipId);

            //Assert
            _commitmentHelper.Verify(x => x.GetApprenticeship(expectedUrl));
            Assert.IsAssignableFrom<Apprenticeship>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingCreateProviderApprenticeship()
        {
            //Arrange
            var expectedCommitmentId = 324236;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}/apprenticeships";

            //Act
            await _providerCommitmentsApi.CreateProviderApprenticeship(ExpectedProviderId, expectedCommitmentId, new ApprenticeshipRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostApprenticeship(expectedUrl, It.IsAny<ApprenticeshipRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingUpdateProviderApprenticeship()
        {
            //Arrange
            var expectedCommitmentId = 324236;
            var expectedApprenticeshipId = 4351;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}/apprenticeships/{expectedApprenticeshipId}";

            //Act
            await _providerCommitmentsApi.UpdateProviderApprenticeship(ExpectedProviderId, expectedCommitmentId,expectedApprenticeshipId, new ApprenticeshipRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PutApprenticeship(expectedUrl, It.IsAny<ApprenticeshipRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingGetProviderCommitmentsAndTheCorrectTypeReturned()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments";
            _commitmentHelper.Setup(x => x.GetCommitments(expectedUrl)).ReturnsAsync(new List<CommitmentListItem>());

            //Act
            var actual = await _providerCommitmentsApi.GetProviderCommitments(ExpectedProviderId);

            //Assert
            _commitmentHelper.Verify(x => x.GetCommitments(expectedUrl));
            Assert.IsAssignableFrom<List<CommitmentListItem>>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingGetProviderCommitmentAndTheCorrectTypeReturned()
        {
            //Arrange
            var expectedCommitmentId = 324236;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}";
            _commitmentHelper.Setup(x => x.GetCommitment(expectedUrl)).ReturnsAsync(new CommitmentView());

            //Act
            var actual = await _providerCommitmentsApi.GetProviderCommitment(ExpectedProviderId, expectedCommitmentId);

            //Assert
            _commitmentHelper.Verify(x => x.GetCommitment(expectedUrl));
            Assert.IsAssignableFrom<CommitmentView>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingBulkUploadApprenticeships()
        {
            //Arrange
            var expectedCommitmentId = 324236;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}/apprenticeships/bulk";

            //Act
            await _providerCommitmentsApi.BulkUploadApprenticeships(ExpectedProviderId, expectedCommitmentId, new BulkApprenticeshipRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostApprenticeships(expectedUrl, It.IsAny<BulkApprenticeshipRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingDeleteProviderApprenticeship()
        {
            //Arrange
            var expectedApprenticeshipId = 4351;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/{expectedApprenticeshipId}";

            //Act
            await _providerCommitmentsApi.DeleteProviderApprenticeship(ExpectedProviderId, expectedApprenticeshipId, new DeleteRequest());

            //Assert
            _commitmentHelper.Verify(x => x.DeleteApprenticeship(expectedUrl, It.IsAny<DeleteRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingDeleteProviderCommitment()
        {
            //Arrange
            var expectedCommitmentId = 4351;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/commitments/{expectedCommitmentId}";

            //Act
            await _providerCommitmentsApi.DeleteProviderCommitment(ExpectedProviderId, expectedCommitmentId, new DeleteRequest());

            //Assert
            _commitmentHelper.Verify(x => x.DeleteCommitment(expectedUrl, It.IsAny<DeleteRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingCreateApprenticeshipUpdate()
        {
            //Arrange
            var expectedApprenticeshipId = 4351;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/{expectedApprenticeshipId}/update";

            //Act
            await _providerCommitmentsApi.CreateApprenticeshipUpdate(ExpectedProviderId, expectedApprenticeshipId, new ApprenticeshipUpdateRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostApprenticeshipUpdate(expectedUrl, It.IsAny<ApprenticeshipUpdateRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingGetPendingApprenticeshipUpdateAndTheCorrectTypeReturned()
        {
            //Arrange
            var expectedApprenticeshipId = 324236;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/{expectedApprenticeshipId}/update";
            _commitmentHelper.Setup(x => x.GetApprenticeshipUpdate(expectedUrl)).ReturnsAsync(new ApprenticeshipUpdate());

            //Act
            var actual = await _providerCommitmentsApi.GetPendingApprenticeshipUpdate(ExpectedProviderId, expectedApprenticeshipId);

            //Assert
            _commitmentHelper.Verify(x => x.GetApprenticeshipUpdate(expectedUrl));
            Assert.IsAssignableFrom<ApprenticeshipUpdate>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsUsedWhenCallingPatchApprenticeshipUpdate()
        {
            //Arrange
            var expectedApprenticeshipId = 4351;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/provider/{ExpectedProviderId}/apprenticeships/{expectedApprenticeshipId}/update";

            //Act
            await _providerCommitmentsApi.PatchApprenticeshipUpdate(ExpectedProviderId, expectedApprenticeshipId, new ApprenticeshipUpdateSubmission());

            //Assert
            _commitmentHelper.Verify(x => x.PatchApprenticeshipUpdate(expectedUrl, It.IsAny<ApprenticeshipUpdateSubmission>()));
        }
    }
}
