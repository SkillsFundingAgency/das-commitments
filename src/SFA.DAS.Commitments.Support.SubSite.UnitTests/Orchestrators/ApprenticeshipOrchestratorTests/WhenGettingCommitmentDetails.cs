using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingCommitmentDetails
    {
        [Test, MoqAutoData]
        public void WhenGettingCommitmentDetails_InvalidCommitmentId_ShouldThrow(
           string hashedCommitmentId,
           string hashedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            encodingServiceMock
                .Setup(o => o.Decode(hashedCommitmentId, EncodingType.CohortReference))
                .Throws(new Exception("Bad commitment ID"));

            // Act
            Assert.ThrowsAsync<Exception>(() => sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId));
        }

        [Test, MoqAutoData]
        public void WhenGettingCommitmentDetails_InvalidAccountId_ShouldThrow(
           string hashedCommitmentId,
           string hashedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            encodingServiceMock
                .Setup(o => o.Decode(hashedAccountId, EncodingType.AccountId))
                .Throws(new Exception("Bad account ID"));

            // Act
            Assert.ThrowsAsync<Exception>(() => sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId));
        }

        [Test, MoqAutoData]
        public void WhenGettingCommitmentDetails_CohortNotFound_ShouldThrow(
           string hashedCommitmentId,
           string hashedAccountId,
           long decodedCommitmentId, 
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            SetupEncodingMocks(hashedCommitmentId, hashedAccountId, decodedCommitmentId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportCohortSummaryQuery>(q => q.CohortId == decodedCommitmentId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync((GetSupportCohortSummaryQueryResult)null);

            // Act
            Assert.ThrowsAsync<Exception>(() => sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId));
        }

        [Test, MoqAutoData]
        public async Task WhenGettingCommitmentDetails_CohortFound_ShouldGetApprenticeshipsForCohort(
           string hashedCommitmentId,
           string hashedAccountId,
           long decodedCommitmentId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           GetSupportCohortSummaryQueryResult getSupportCohortSummaryQueryResult,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            getSupportCohortSummaryQueryResult.CohortId = decodedCommitmentId;
            SetupEncodingMocks(hashedCommitmentId, hashedAccountId, decodedCommitmentId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportCohortSummaryQuery>(q => q.CohortId == decodedCommitmentId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(getSupportCohortSummaryQueryResult);

            // Act
            var result = await sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId);

            // Assert
            mediatorMock.Verify(m => m.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.AccountId == decodedAccountId && q.CohortId == decodedCommitmentId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenGettingCommitmentDetails_CohortFound_ApprenticeshipsFound_ShouldMapResults(
           string hashedCommitmentId,
           string hashedAccountId,
           long decodedCommitmentId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<ICommitmentMapper> commitmentMapperMock,
           GetSupportCohortSummaryQueryResult getSupportCohortSummaryQueryResult,
           GetSupportApprenticeshipQueryResult getSupportApprenticeshipQueryResult,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            getSupportCohortSummaryQueryResult.CohortId = decodedCommitmentId;
            SetupEncodingMocks(hashedCommitmentId, hashedAccountId, decodedCommitmentId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportCohortSummaryQuery>(q => q.CohortId == decodedCommitmentId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(getSupportCohortSummaryQueryResult);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.AccountId == decodedAccountId && q.CohortId == decodedCommitmentId), CancellationToken.None))
                .ReturnsAsync(getSupportApprenticeshipQueryResult);

            // Act
            var result = await sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId);

            // Assert
            commitmentMapperMock.Verify(m => m.MapToCommitmentDetailViewModel(getSupportCohortSummaryQueryResult, getSupportApprenticeshipQueryResult), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenGettingCommitmentDetails_CohortFound_ApprenticeshipsFound_ShouldReturnMappedObject(
           string hashedCommitmentId,
           string hashedAccountId,
           long decodedCommitmentId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<ICommitmentMapper> commitmentMapperMock,
           GetSupportCohortSummaryQueryResult getSupportCohortSummaryQueryResult,
           GetSupportApprenticeshipQueryResult getSupportApprenticeshipQueryResult,
           CommitmentDetailViewModel commitmentDetailViewModel,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            getSupportCohortSummaryQueryResult.CohortId = decodedCommitmentId;
            SetupEncodingMocks(hashedCommitmentId, hashedAccountId, decodedCommitmentId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportCohortSummaryQuery>(q => q.CohortId == decodedCommitmentId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(getSupportCohortSummaryQueryResult);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.AccountId == decodedAccountId && q.CohortId == decodedCommitmentId), CancellationToken.None))
                .ReturnsAsync(getSupportApprenticeshipQueryResult);

            commitmentMapperMock.Setup(m => m.MapToCommitmentDetailViewModel(getSupportCohortSummaryQueryResult, getSupportApprenticeshipQueryResult)).Returns(commitmentDetailViewModel);

            // Act
            var result = await sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId);

            // Assert
            result.Should().Be(commitmentDetailViewModel);
        }

        private static void SetupEncodingMocks(string hashedCommitmentId, string hashedAccountId, long decodedCommitmentId, long decodedAccountId, Mock<IEncodingService> encodingServiceMock)
        {
            encodingServiceMock
                .Setup(o => o.Decode(hashedCommitmentId, EncodingType.CohortReference))
                .Returns(decodedCommitmentId);

            encodingServiceMock
                .Setup(o => o.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);
        }
    }
}