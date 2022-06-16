using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Logging;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators.ApprenticeshipOrchestratorTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingCommitmentSummary
    {
        private Mock<IMediator> _mediator;
        private Mock<IValidator<ApprenticeshipSearchQuery>> _searchValidator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private Mock<IEncodingService> _encodingService;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private GetSupportCohortSummaryQueryResult _mockedCommitmentResult;
        private GetSupportApprenticeshipQueryResult _mockedSupportApprenticeshipResult;
        private ApprenticeshipsOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _mediator = new Mock<IMediator>();
            _searchValidator = new Mock<IValidator<ApprenticeshipSearchQuery>>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _encodingService = new Mock<IEncodingService>();
            _commitmentMapper = new Mock<ICommitmentMapper>();

            _encodingService
                .Setup(o => o.Decode(It.IsAny<string>(), It.IsAny<EncodingType>()))
                .Returns(100);

            _encodingService
             .Setup(o => o.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()))
             .Returns("ABCDE500");

            var dataFixture = new Fixture();
            _mockedCommitmentResult = dataFixture.Build<GetSupportCohortSummaryQueryResult>().Create();
            _mockedSupportApprenticeshipResult = dataFixture.Build<GetSupportApprenticeshipQueryResult>().Create();

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                            _mediator.Object,
                            _apprenticeshipMapper.Object,
                            _searchValidator.Object,
                            _encodingService.Object,
                            _commitmentMapper.Object);
        }

        [Test]
        [Category("UnitTest")]
        public async Task GivenInvalidCohortShouldReturnResponseMessageAndNotCallSearchService()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new Models.ApprenticeshipSearchQuery
            {
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportCohortSummaryQueryResult()).Verifiable();

            var validationFailures = new List<ValidationFailure>
            {
               new ValidationFailure("SearchTerm","Invalid Cohort")
            };

            var validationResult = new ValidationResult(validationFailures);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult)
                .Verifiable();

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
             _mediator.Object,
             _apprenticeshipMapper.Object,
             _searchValidator.Object,
             _encodingService.Object,
             _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            _searchValidator.VerifyAll();
            _mediator.Verify(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().HaveCount(1);
        }

        [Test]
        [Category("UnitTest")]
        public async Task GivenCohortHashWhichCannotBeDecodedShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
             .ReturnsAsync(new GetSupportCohortSummaryQueryResult()).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _encodingService
                .Setup(x => x.Decode(searchQuery.SearchTerm, It.IsAny<EncodingType>()))
                .Throws<Exception>();

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                 _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().Contain("Please enter a valid Cohort number");
        }

        [Test]
        [TestCase(true, Description = "Response returned as null")]
        [Category("UnitTest")]
        public async Task NoCohortFoundShouldReturnResponseMessage(bool responseAsNull)
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            var getComtResponse = responseAsNull ? null : new GetSupportCohortSummaryQueryResult();

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
           .ReturnsAsync(getComtResponse).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().Contain("No record Found");
        }

        [Test]
        [Category("UnitTest")]
        public async Task CohortFoundShouldReturnCohortCommitmentSummary()
        {
            // Arrange
            const string employerName = "Employer Name";
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HashedAccountId",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportCohortSummaryQueryResult
            {
                AccountId = 100
            })
            .Verifiable();

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = new List<CommitmentsV2.Models.SupportApprenticeshipDetails>()
            });

            _encodingService
            .Setup(o => o.Decode(searchQuery.HashedAccountId, EncodingType.AccountId))
            .Returns(100);

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery)).Returns(validationResult.Object);

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);

            _commitmentMapper
                .Setup(x => x.MapToCommitmentSummaryViewModel(It.IsAny<GetSupportCohortSummaryQueryResult>(), It.IsAny<GetSupportApprenticeshipQueryResult>()))
                .Returns(new CommitmentSummaryViewModel
                {
                    EmployerName = employerName
                });

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().BeNullOrEmpty();
            result.EmployerName.Should().Be(employerName);
        }

        [Test]
        [Category("UnitTest")]
        public async Task QueryThrowsUnauthorizedShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator
                .Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
                .Throws<Exception>();

            var validationResult = new Mock<ValidationResult>();
            validationResult
                .SetupGet(x => x.IsValid)
                .Returns(true);

            _searchValidator
                .Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
               _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().Contain("Unable to load resource error");
        }

        [Test]
        [Category("UnitTest")]
        public async Task GivenAccountHashIdCannotBeDecodedShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "BADHASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
             .ReturnsAsync(new GetSupportCohortSummaryQueryResult()).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _encodingService
                .Setup(x => x.Decode(searchQuery.HashedAccountId, EncodingType.AccountId))
                .Throws<Exception>();

            _orchestrator = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                 _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
                _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().Contain("Problem validating your account Id");
        }

        [Test]
        [Category("UnitTest")]
        public async Task GivenCohortAccountIdDoNotMatchQueryAccountIdShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "100HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            long cohortCorrectAccountId = 200;
            long cohortInvalidAccountId = 300;

            GetSupportCohortSummaryQueryResult getSupportCohortSummaryQueryResult = new Fixture().Create<GetSupportCohortSummaryQueryResult>();
            getSupportCohortSummaryQueryResult.AccountId = cohortCorrectAccountId;

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportCohortSummaryQuery>(), CancellationToken.None))
             .ReturnsAsync(getSupportCohortSummaryQueryResult);

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _encodingService
                .Setup(x => x.Decode(searchQuery.HashedAccountId, EncodingType.AccountId))
                .Returns(cohortInvalidAccountId);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().Contain("Account is unauthorised to access this Cohort.");
        }
    }
}