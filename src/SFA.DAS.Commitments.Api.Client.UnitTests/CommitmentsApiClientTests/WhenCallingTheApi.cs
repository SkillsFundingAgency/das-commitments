using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.CommitmentsApiClientTests
{
    public class WhenCallingTheApi
    {
        private Mock<IHttpCommitmentHelper> _commitmentHelper;
        private Mock<ICommitmentsApiClientConfiguration> _configuration;
        private EmployerCommitmentApi _employerCommitmentApi;

        private const string ExpectedApiBaseUrl = "http://local.test";
        private const long ExpectedAccountId = 587458;

        [SetUp]
        public void Arrange()
        {

            _commitmentHelper = new Mock<IHttpCommitmentHelper>();
            _configuration = new Mock<ICommitmentsApiClientConfiguration>();
            _configuration.Setup(x => x.BaseUrl).Returns(ExpectedApiBaseUrl);

            _employerCommitmentApi = new EmployerCommitmentApi(_configuration.Object, _commitmentHelper.Object);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForCreateEmployerCommitment()
        {

            //Act
            await _employerCommitmentApi.CreateEmployerCommitment(ExpectedAccountId, new CommitmentRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostCommitment($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments", It.IsAny<CommitmentRequest>()));
        }

        [Test]
        public async Task ThenTheCommitmentViewIsReturnedInTheResponseWhenCreateEmployerCommitmentIsCalled()
        {
            //Arrange
            _commitmentHelper.Setup(x => x.PostCommitment($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments", It.IsAny<CommitmentRequest>())).ReturnsAsync(new CommitmentView());

            //Act
            var actual = await _employerCommitmentApi.CreateEmployerCommitment(ExpectedAccountId, new CommitmentRequest());

            //Assert
            Assert.IsAssignableFrom<CommitmentView>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetEmployerCommitmentsAndCorrectTypeIsReturnedInTheResponse()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments";
            _commitmentHelper.Setup(x => x.GetCommitments(expectedUrl)).ReturnsAsync(new List<CommitmentListItem>());

            //Act
            var actual = await _employerCommitmentApi.GetEmployerCommitments(ExpectedAccountId);

            //Assert
            _commitmentHelper.Verify(x => x.GetCommitments(expectedUrl));
            Assert.IsAssignableFrom<List<CommitmentListItem>>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetEmployerCommitmentAndTheCorrectTypeIsReturnedInTheResponse()
        {
            //Arrange
            var expectedCommitmentId = 445345;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments/{expectedCommitmentId}";
            _commitmentHelper.Setup(x => x.GetCommitment(expectedUrl)).ReturnsAsync(new CommitmentView());

            //Act
            var actual = await _employerCommitmentApi.GetEmployerCommitment(ExpectedAccountId, expectedCommitmentId);

            //Assert
            _commitmentHelper.Verify(x => x.GetCommitment(expectedUrl));
            Assert.IsAssignableFrom<CommitmentView>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetEmployerApprenticeshipsAndTheCorrectTypeIsReturnedInTheResponse()
        {
            //Arrange
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/";
            _commitmentHelper.Setup(x => x.GetApprenticeships(expectedUrl)).ReturnsAsync(new List<Apprenticeship>());

            //Act
            var actual = await _employerCommitmentApi.GetEmployerApprenticeships(ExpectedAccountId);

            //Assert
            _commitmentHelper.Verify(x => x.GetApprenticeships(expectedUrl));
            Assert.IsAssignableFrom<List<Apprenticeship>>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetEmployerApprenticeshipAndTheCorrectTypeIsReturnedInTheResponse()
        {
            //Arrange
            var expectedApprenticeshipId = 63498;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}";
            _commitmentHelper.Setup(x => x.GetApprenticeship(expectedUrl)).ReturnsAsync(new Apprenticeship());

            //Act
            var actual = await _employerCommitmentApi.GetEmployerApprenticeship(ExpectedAccountId, expectedApprenticeshipId);

            //Assert
            _commitmentHelper.Verify(x => x.GetApprenticeship(expectedUrl));
            Assert.IsAssignableFrom<Apprenticeship>(actual);
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForPatchEmployerCommitment()
        {
            //Arrange
            var expectedCommitmentId = 63498;

            //Act
            await _employerCommitmentApi.PatchEmployerCommitment(ExpectedAccountId, expectedCommitmentId, new CommitmentSubmission());

            //Assert
            _commitmentHelper.Verify(x => x.PatchCommitment($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments/{expectedCommitmentId}", It.IsAny<CommitmentSubmission>()));

        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForCreateEmployerApprenticeship()
        {
            //Arrange
            var expectedCommitmentId = 63498;

            //Act
            await _employerCommitmentApi.CreateEmployerApprenticeship(ExpectedAccountId, expectedCommitmentId, new ApprenticeshipRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostApprenticeship($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments/{expectedCommitmentId}/apprenticeships", It.IsAny<ApprenticeshipRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForUpdateEmployerApprenticeship()
        {
            //Arrange
            var expectedCommitmentId = 63498;
            var expectedApprenticeshipId = 993344;

            //Act
            await _employerCommitmentApi.UpdateEmployerApprenticeship(ExpectedAccountId, expectedCommitmentId, expectedApprenticeshipId, new ApprenticeshipRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PutApprenticeship($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments/{expectedCommitmentId}/apprenticeships/{expectedApprenticeshipId}", It.IsAny<ApprenticeshipRequest>()));
        }


        [Test]
        public async Task ThenTheCorrectUrlIsCalledForPatchEmployerApprenticeship()
        {
            //Arrange
            var expectedApprenticeshipId = 993344;

            //Act
            await _employerCommitmentApi.PatchEmployerApprenticeship(ExpectedAccountId, expectedApprenticeshipId, new ApprenticeshipSubmission());

            //Assert
            _commitmentHelper.Verify(x => x.PatchApprenticeship($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}", It.IsAny<ApprenticeshipSubmission>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForDeleteApprenticeship()
        {
            //Arrange
            var expectedApprenticeshipId = 993344;

            //Act
            await _employerCommitmentApi.DeleteEmployerApprenticeship(ExpectedAccountId, expectedApprenticeshipId, new DeleteRequest());

            //Assert
            _commitmentHelper.Verify(x => x.DeleteApprenticeship($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}", It.IsAny<DeleteRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForDeleteCommitment()
        {
            //Arrange
            var expectedCommitmentId = 993344;

            //Act
            await _employerCommitmentApi.DeleteEmployerCommitment(ExpectedAccountId, expectedCommitmentId, new DeleteRequest());

            //Assert
            _commitmentHelper.Verify(x => x.DeleteCommitment($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/commitments/{expectedCommitmentId}", It.IsAny<DeleteRequest>()));
        }


        [Test]
        public async Task ThenTheCorrectUrlIsCalledForCreateApprenticeshipUpdate()
        {
            //Arrange
            var expectedApprenticeshipId = 993344;

            //Act
            await _employerCommitmentApi.CreateApprenticeshipUpdate(ExpectedAccountId, expectedApprenticeshipId, new ApprenticeshipUpdateRequest());

            //Assert
            _commitmentHelper.Verify(x => x.PostApprenticeshipUpdate($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}/update", It.IsAny<ApprenticeshipUpdateRequest>()));
        }

        [Test]
        public async Task ThenTheCorrectUrlIsCalledForGetPendingApprenticeshipUpdateAndTheCorrectTypeIsReturned()
        {
            //Arrange
            var expectedApprenticeshipId = 993344;
            var expectedUrl = $"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}/update";
            _commitmentHelper.Setup(x => x.GetApprenticeshipUpdate(expectedUrl)).ReturnsAsync(new ApprenticeshipUpdate());

            //Act
            var actual = await _employerCommitmentApi.GetPendingApprenticeshipUpdate(ExpectedAccountId, expectedApprenticeshipId);

            //Assert
            Assert.IsAssignableFrom<ApprenticeshipUpdate>(actual);
            _commitmentHelper.Verify(x => x.GetApprenticeshipUpdate(expectedUrl));
        }


        [Test]
        public async Task ThenTheCorrectUrlIsCalledForPatchApprenticeshipUpdate()
        {
            //Arrange
            var expectedApprenticeshipId = 993344;

            //Act
            await _employerCommitmentApi.PatchApprenticeshipUpdate(ExpectedAccountId, expectedApprenticeshipId, new ApprenticeshipUpdateSubmission());

            //Assert
            _commitmentHelper.Verify(x => x.PatchApprenticeshipUpdate($"{ExpectedApiBaseUrl}/api/employer/{ExpectedAccountId}/apprenticeships/{expectedApprenticeshipId}/update", It.IsAny<ApprenticeshipUpdateSubmission>()));
        }
    }
}
