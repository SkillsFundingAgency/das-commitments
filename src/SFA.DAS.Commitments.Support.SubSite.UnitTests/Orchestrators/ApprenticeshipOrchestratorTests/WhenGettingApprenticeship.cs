using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators.ApprenticeshipOrchestratorTests;

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

    [Test]
    public async Task GivenValidApprenticeshipIdShouldCallRequiredServices()
    {
        // Arrange
        const string hashedApprenticeshipId = "ABC001";
        const string hashedAccountId = "ACCOUNT001";

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = GetApprenticeships()
            }).Verifiable();

        _mediator.Setup(x => x.Send(It.IsAny<GetPriceEpisodesQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetPriceEpisodesQueryResult
            {
                PriceEpisodes = GetPriceEpisodes()
            }).Verifiable();

        _apprenticeshipMapper
            .Setup(o => o.MapToApprenticeshipViewModel(It.IsAny<GetSupportApprenticeshipQueryResult>(), It.IsAny<GetChangeOfProviderChainQueryResult>()))
            .Returns(new ApprenticeshipViewModel())
            .Verifiable();

        _encodingService
            .Setup(o => o.Decode(hashedApprenticeshipId, EncodingType.ApprenticeshipId))
            .Returns(100);

        _encodingService
            .Setup(o => o.Decode(hashedAccountId, EncodingType.AccountId))
            .Returns(100);

        // Act
        await _sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

        // Assert
        _encodingService.Verify(o => o.Decode(hashedApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        _encodingService.Verify(o => o.Decode(hashedAccountId, EncodingType.AccountId), Times.Once);

        _mediator.Verify(x => x.Send(It.Is<GetSupportApprenticeshipQuery>(o => o.ApprenticeshipId == 100), CancellationToken.None), Times.Once);
        _mediator.Verify(x => x.Send(It.Is<GetPriceEpisodesQuery>(o => o.ApprenticeshipId == 100), CancellationToken.None), Times.Once);
        _apprenticeshipMapper.Verify(o => o.MapToApprenticeshipViewModel(It.IsAny<GetSupportApprenticeshipQueryResult>(), It.IsAny<GetChangeOfProviderChainQueryResult>()), Times.Once);
    }

    [Test]
    public async Task GivenValidApprenticeshipIdShouldGetApprenticeshipUpdate()
    {
        // Arrange
        const string hashedApprenticeshipId = "ABC001";
        const string hashedAccountId = "ACCOUNT001";

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = GetApprenticeships()
            }).Verifiable();
        
        _mediator.Setup(x => x.Send(It.IsAny<GetPriceEpisodesQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetPriceEpisodesQueryResult
            {
                PriceEpisodes = GetPriceEpisodes()
            }).Verifiable();

        _apprenticeshipMapper
            .Setup(o => o.MapToApprenticeshipViewModel(It.IsAny<GetSupportApprenticeshipQueryResult>(), It.IsAny<GetChangeOfProviderChainQueryResult>()))
            .Returns(new ApprenticeshipViewModel())
            .Verifiable();

        _apprenticeshipMapper
            .Setup(o => o.MapToUpdateApprenticeshipViewModel(It.IsAny<GetApprenticeshipUpdateQueryResult>(), It.IsAny<SupportApprenticeshipDetails>()))
            .Returns(new ApprenticeshipUpdateViewModel());

        _encodingService
            .Setup(o => o.Decode(hashedApprenticeshipId, EncodingType.ApprenticeshipId))
            .Returns(100);

        _encodingService
            .Setup(o => o.Decode(hashedAccountId, EncodingType.AccountId))
            .Returns(100);

        // Act
        var result = await _sut.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);

        _apprenticeshipMapper.Verify(o => o.MapToUpdateApprenticeshipViewModel(It.IsAny<GetApprenticeshipUpdateQueryResult>(), It.IsAny<SupportApprenticeshipDetails>()), Times.Once);
    }

    [Test]
    public async Task GivenValidUlnShouldCallRequiredServices()
    {
        // Arrange
        var searchQuery = new ApprenticeshipSearchQuery
        {
            SearchTerm = "1000201219",
            SearchType = ApprenticeshipSearchType.SearchByUln,
            HashedAccountId = "ABC1234"
        };

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = GetApprenticeships()
            }).Verifiable();

        var validationResult = new Mock<ValidationResult>();
        validationResult.SetupGet(x => x.IsValid).Returns(true);

        _searchValidator.Setup(x => x.Validate(searchQuery))
            .Returns(validationResult.Object)
            .Verifiable();

        _apprenticeshipMapper
            .Setup(o => o.MapToUlnResultView(It.IsAny<GetSupportApprenticeshipQueryResult>()))
            .Returns(new UlnSummaryViewModel())
            .Verifiable();

        _encodingService
            .Setup(o => o.Decode(searchQuery.HashedAccountId, EncodingType.AccountId))
            .Returns(100);

        // Act
        await _sut.GetApprenticeshipsByUln(searchQuery);

        // Assert
        _encodingService.Verify(o => o.Decode(searchQuery.HashedAccountId, EncodingType.AccountId), Times.Once);

        _searchValidator.VerifyAll();
        _mediator.VerifyAll();
        _apprenticeshipMapper.VerifyAll();
    }

    [Test]
    public async Task GivenInvalidHashedAccountIdReturnErrorResponseMessage()
    {
        // Arrange
        var searchQuery = new ApprenticeshipSearchQuery
        {
            SearchTerm = "1000201219",
            SearchType = ApprenticeshipSearchType.SearchByUln
        };

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = GetApprenticeships()
            });

        var validationResult = new Mock<ValidationResult>();
        validationResult.SetupGet(x => x.IsValid).Returns(true);

        _searchValidator.Setup(x => x.Validate(searchQuery))
            .Returns(validationResult.Object)
            .Verifiable();

        _encodingService
            .Setup(o => o.Decode(searchQuery.HashedAccountId, EncodingType.AccountId))
            .Throws(new Exception());

        // Act
        var result = await _sut.GetApprenticeshipsByUln(searchQuery);

        // Assert
        _searchValidator.VerifyAll();
        _mediator.Verify(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None), Times.Never);

        result.Should().NotBeNull();
        result.Should().BeOfType<UlnSummaryViewModel>();

        result.ReponseMessages.Should().NotBeNull();
        result.ReponseMessages.Should().HaveCount(1);
    }

    [Test]
    public async Task GivenInvalidUlnShouldReturnResponseMessageAndNotCallSearchService()
    {
        // Arrange
        var searchQuery = new ApprenticeshipSearchQuery
        {
            SearchTerm = "000000001",
            SearchType = ApprenticeshipSearchType.SearchByUln
        };

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = GetApprenticeships()
            });

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("SearchTerm", "Invalid Uln")
        };

        var validationResult = new ValidationResult(validationFailures);

        _searchValidator.Setup(x => x.Validate(searchQuery))
            .Returns(validationResult)
            .Verifiable();

        // Act
        var result = await _sut.GetApprenticeshipsByUln(searchQuery);

        // Assert
        _searchValidator.VerifyAll();
        _mediator.Verify(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None), Times.Never);

        result.Should().NotBeNull();
        result.Should().BeOfType<UlnSummaryViewModel>();

        result.ReponseMessages.Should().NotBeNull();
        result.ReponseMessages.Should().HaveCount(1);
    }

    [Test]
    public async Task WhenNoUlnRecordIsFoundShouldReturnResponseMessages()
    {
        // Arrange
        var searchQuery = new ApprenticeshipSearchQuery
        {
            SearchTerm = "1000201219",
            SearchType = ApprenticeshipSearchType.SearchByUln
        };

        _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = null
            });

        var validationResult = new Mock<ValidationResult>();
        validationResult.SetupGet(x => x.IsValid).Returns(true);

        _searchValidator.Setup(x => x.Validate(searchQuery))
            .Returns(validationResult.Object)
            .Verifiable();

        // Act
        var result = await _sut.GetApprenticeshipsByUln(searchQuery);

        // Assert
        _searchValidator.VerifyAll();
        _mediator.Verify(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None), Times.Once);

        result.Should().NotBeNull();
        result.Should().BeOfType<UlnSummaryViewModel>();

        result.ReponseMessages.Should().NotBeNull();
        result.ReponseMessages.Should().HaveCount(1);
    }

    private static List<SupportApprenticeshipDetails> GetApprenticeships()
    {
        return
        [
            new()
            {
                FirstName = "Testoo1",
                StartDate = new DateTime(2020, 1, 1)
            }
        ];
    }

    private static IReadOnlyCollection<GetPriceEpisodesQueryResult.PriceEpisode> GetPriceEpisodes()
    {
        return new[]
        {
            new GetPriceEpisodesQueryResult.PriceEpisode
            {
                ApprenticeshipId = 1,
                Cost = 100,
                FromDate = DateTime.Today.AddDays(-10),
                ToDate = DateTime.Today.AddDays(10),
                TrainingPrice = 100,
                Id = 11,
            }
        };
    }
}