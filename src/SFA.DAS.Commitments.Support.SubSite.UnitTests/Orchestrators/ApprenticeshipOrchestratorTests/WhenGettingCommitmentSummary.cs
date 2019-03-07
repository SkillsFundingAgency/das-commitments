using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators.ApprenticeshipOrchestratorTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingCommitmentSummary
    {
        private Mock<ILog> _logger;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<ApprenticeshipSearchQuery>> _searchValidator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private Mock<IHashingService> _hashingService;
        private Mock<ICommitmentMapper> _commitmentMapper;

        [SetUp]
        public void OneTimeSetup()
        {
            _logger = new Mock<ILog>();
            _mediator = new Mock<IMediator>();
            _searchValidator = new Mock<IValidator<ApprenticeshipSearchQuery>>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _hashingService = new Mock<IHashingService>();
            _commitmentMapper = new Mock<ICommitmentMapper>();

            _hashingService
                .Setup(o => o.DecodeValue(It.IsAny<string>()))
                .Returns(100);

            _hashingService
             .Setup(o => o.HashValue(It.IsAny<long>()))
             .Returns("ABCDE500");

            _logger.Setup(x => x.Trace(It.IsAny<string>()));
            _logger.Setup(x => x.Info(It.IsAny<string>()));
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

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
            .ReturnsAsync(new GetCommitmentResponse
            {
                Data = new Commitment { }
            }).Verifiable();

            var validationFailures = new List<ValidationFailure>
            {
               new ValidationFailure("SearchTerm","Invalid Cohort")
            };

            var validationResult = new ValidationResult(validationFailures);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult)
                .Verifiable();

            var _orchestrator = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
                _commitmentMapper.Object);

            // Act
            var result = await _orchestrator.GetCommitmentSummary(searchQuery);

            // Assert
            _searchValidator.VerifyAll();
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()), Times.Never);

            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().HaveCount(1);
        }

        [Test]
        [Category("UnitTest")]
        public async Task GivenInvalidHashedAccountIdShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
            .ReturnsAsync(new GetCommitmentResponse
            {
                Data = new Commitment { }
            }).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _hashingService.Setup(x => x.DecodeValue(It.Is<string>(s => s == searchQuery.SearchTerm))).Returns(1234);
            _hashingService.Setup(x => x.DecodeValue(searchQuery.HashedAccountId)).Throws<Exception>();

            var _orchestrator = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
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
        public async Task GivenCohortHashWhichCannotBeDecodedShouldReturnResponseMessage()
        {
            // Arrange
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
            .ReturnsAsync(new GetCommitmentResponse
            {
                Data = new Commitment { }
            }).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            _hashingService.Setup(x => x.DecodeValue(searchQuery.SearchTerm)).Throws<Exception>();

            var _orchestrator = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
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
        [TestCase(false, Description = "Data returned as null")]
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

            var getComtResponse = responseAsNull ? null : new GetCommitmentResponse { Data = null };
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
            .ReturnsAsync(getComtResponse);

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            var _orchestrator = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
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
                HashedAccountId = "HASH",
                SearchTerm = "short",
                SearchType = ApprenticeshipSearchType.SearchByCohort
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
            .ReturnsAsync(new GetCommitmentResponse
            {
                Data = new Commitment()
            });

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            var _orchestrator = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
                _commitmentMapper.Object);

            _commitmentMapper.Setup(x => x.MapToCommitmentSummaryViewModel(It.IsAny<Commitment>())).Returns(new CommitmentSummaryViewModel
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
                .Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>()))
                .Throws<UnauthorizedException>();

            var validationResult = new Mock<ValidationResult>();
            validationResult
                .SetupGet(x => x.IsValid)
                .Returns(true);

            _searchValidator
                .Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object);

            var _orchestrator = new ApprenticeshipsOrchestrator(
                _logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
                _commitmentMapper.Object);

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
