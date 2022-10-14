using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingApprenticeship
    {
        private Mock<IMediator> _mediator;
        private Mock<IValidator<ApprenticeshipSearchQuery>> _searchValidator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private Mock<IEncodingService> _encodingService;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private ApprenticeshipsOrchestrator _sut;

        [SetUp]
        public void Setup()
        {
            _mediator = new Mock<IMediator>();
            _searchValidator = new Mock<IValidator<ApprenticeshipSearchQuery>>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _encodingService = new Mock<IEncodingService>();
            _commitmentMapper = new Mock<ICommitmentMapper>();

            _apprenticeshipMapper
              .Setup(o => o.MapToUlnResultView(It.IsAny<GetSupportApprenticeshipQueryResult>()))
              .Returns(new UlnSummaryViewModel())
              .Verifiable();

            _sut = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);
        }

        [Test, MoqAutoData]
        public void GivenValidApprenticeshipId_NoApprenticeshipsReturned_ShouldError(
            string hashedApprenticeshipId,
            string hashedAccountId,
            ApprenticeshipsOrchestrator sut)
        {
            // Arrange

            // Act
            Assert.ThrowsAsync<Exception>(() => sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId));

            // Assert
        }

        [Test, MoqAutoData]
        public async Task GivenValidApprenticeshipId_ApprenticeshipsReturned_ShouldReturnApprenticeships(
           string hashedApprenticeshipId,
           string hashedAccountId,
           long decodedApprenticeshipId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<IApprenticeshipMapper> apprenticeshipMapperMock,
           GetSupportApprenticeshipQueryResult supportApprenticeshipQueryResult,
           ApprenticeshipViewModel apprenticeshipViewModel,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            SetupEncodingMocks(hashedApprenticeshipId, hashedAccountId, decodedApprenticeshipId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.ApprenticeshipId == decodedApprenticeshipId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(supportApprenticeshipQueryResult);

            SetupMapperMocks(apprenticeshipMapperMock, apprenticeshipViewModel);

            // Act
            var result = await sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

            // Assert
            result.Should().Be(apprenticeshipViewModel);
        }

        [Test, MoqAutoData]
        public async Task GivenValidApprenticeshipId_ApprenticeshipsReturned_WithUpdate_NotCost_ShouldReturnApprenticeshipWithUpdate(
           string hashedApprenticeshipId,
           string hashedAccountId,
           long decodedApprenticeshipId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<IApprenticeshipMapper> apprenticeshipMapperMock,
           GetSupportApprenticeshipQueryResult supportApprenticeshipQueryResult,
           ApprenticeshipViewModel apprenticeshipViewModel,
           ApprenticeshipUpdateViewModel apprenticeshipUpdateViewModel,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            apprenticeshipUpdateViewModel.Cost = null;

            SetupEncodingMocks(hashedApprenticeshipId, hashedAccountId, decodedApprenticeshipId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.ApprenticeshipId == decodedApprenticeshipId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(supportApprenticeshipQueryResult);

            SetupMapperMocks(apprenticeshipMapperMock, apprenticeshipViewModel, apprenticeshipUpdateViewModel);

            // Act
            var result = await sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

            // Assert
            result.ApprenticeshipUpdates.Should().Be(apprenticeshipUpdateViewModel);
        }

        [Test, MoqAutoData]
        public async Task GivenValidApprenticeshipId_ApprenticeshipsReturned_WithUpdate_WithCost_ShouldReturnApprenticeshipWithUpdate_And_TrainingCost(
           string hashedApprenticeshipId,
           string hashedAccountId,
           long decodedApprenticeshipId,
           long decodedAccountId,
           decimal trainingCost,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<IApprenticeshipMapper> apprenticeshipMapperMock,
           GetSupportApprenticeshipQueryResult supportApprenticeshipQueryResult,

           ApprenticeshipViewModel apprenticeshipViewModel,
           ApprenticeshipUpdateViewModel apprenticeshipUpdateViewModel,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            GetPriceEpisodesQueryResult getPriceEpisodesQueryResult = CreatePriceEpisodesForApprenticeship(decodedApprenticeshipId, trainingCost);

            SetupEncodingMocks(hashedApprenticeshipId, hashedAccountId, decodedApprenticeshipId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.ApprenticeshipId == decodedApprenticeshipId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(supportApprenticeshipQueryResult);

            SetupMapperMocks(apprenticeshipMapperMock, apprenticeshipViewModel, apprenticeshipUpdateViewModel);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetPriceEpisodesQuery>(q => q.ApprenticeshipId == decodedApprenticeshipId), CancellationToken.None))
                .ReturnsAsync(getPriceEpisodesQueryResult);

            // Act
            var result = await sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

            // Assert
            result.TrainingCost.Should().Be(trainingCost);
        }

        [Test, MoqAutoData]
        public async Task GivenValidApprenticeshipId_ApprenticeshipsReturned_WithOLTD__ShouldReturnApprenticeshipWithOLTD(
           string hashedApprenticeshipId,
           string hashedAccountId,
           long decodedApprenticeshipId,
           long decodedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           [Frozen] Mock<IMediator> mediatorMock,
           [Frozen] Mock<IApprenticeshipMapper> apprenticeshipMapperMock,
           GetSupportApprenticeshipQueryResult supportApprenticeshipQueryResult,
           ApprenticeshipViewModel apprenticeshipViewModel,
           ApprenticeshipUpdateViewModel apprenticeshipUpdateViewModel,
           OverlappingTrainingDateRequestViewModel overlappingTrainingDateRequestViewModel,
           ApprenticeshipsOrchestrator sut)
        {
            // Arrange
            apprenticeshipUpdateViewModel.Cost = null;

            SetupEncodingMocks(hashedApprenticeshipId, hashedAccountId, decodedApprenticeshipId, decodedAccountId, encodingServiceMock);

            mediatorMock
                .Setup(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(q => q.ApprenticeshipId == decodedApprenticeshipId && q.AccountId == decodedAccountId), CancellationToken.None))
                .ReturnsAsync(supportApprenticeshipQueryResult);

            SetupMapperMocks(apprenticeshipMapperMock, apprenticeshipViewModel, apprenticeshipUpdateViewModel, overlappingTrainingDateRequestViewModel);

            // Act
            var result = await sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

            // Assert
            result.OverlappingTrainingDateRequest.Should().Be(overlappingTrainingDateRequestViewModel);
        }

        private static void SetupEncodingMocks(string hashedApprenticeshipId, string hashedAccountId, long decodedApprenticeshipId, long decodedAccountId, Mock<IEncodingService> encodingServiceMock)
        {
            encodingServiceMock
                .Setup(o => o.Decode(hashedApprenticeshipId, EncodingType.ApprenticeshipId))
                .Returns(decodedApprenticeshipId);

            encodingServiceMock
                .Setup(o => o.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);
        }

        private static void SetupMapperMocks(
            Mock<IApprenticeshipMapper> apprenticeshipMapperMock,
            ApprenticeshipViewModel apprenticeshipViewModel,
            ApprenticeshipUpdateViewModel apprenticeshipUpdateViewModel = null,
            OverlappingTrainingDateRequestViewModel overlappingTrainingDateRequestViewModel = null)
        {
            apprenticeshipMapperMock
                .Setup(o => o.MapToApprenticeshipViewModel(It.IsAny<GetSupportApprenticeshipQueryResult>(), It.IsAny<GetChangeOfProviderChainQueryResult>()))
                .Returns(apprenticeshipViewModel);

            apprenticeshipMapperMock
                .Setup(o => o.MapToUpdateApprenticeshipViewModel(It.IsAny<GetApprenticeshipUpdateQueryResult>(), It.IsAny<SupportApprenticeshipDetails>()))
                .Returns(apprenticeshipUpdateViewModel);

            apprenticeshipMapperMock
                .Setup(o => o.MapToOverlappingTrainingDateRequest(It.IsAny<GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest>()))
                .Returns(overlappingTrainingDateRequestViewModel);
        }

        private static GetPriceEpisodesQueryResult CreatePriceEpisodesForApprenticeship(long decodedApprenticeshipId, decimal latestPriceEpisodeCost)
        {
            return new GetPriceEpisodesQueryResult
            {
                PriceEpisodes = new List<GetPriceEpisodesQueryResult.PriceEpisode>
                {
                    new GetPriceEpisodesQueryResult.PriceEpisode
                    {
                        Id = 1,
                        ApprenticeshipId = decodedApprenticeshipId,
                        FromDate = DateTime.Now.AddYears(-1),
                        ToDate = DateTime.Now.AddMonths(-6).AddDays(-1),
                        Cost = 11229944
                    },
                    new GetPriceEpisodesQueryResult.PriceEpisode
                    {
                        Id = 2,
                        ApprenticeshipId = decodedApprenticeshipId,
                        FromDate = DateTime.Now.AddMonths(-6),
                        Cost = latestPriceEpisodeCost
                    }
                }
            };
        }
    }
}