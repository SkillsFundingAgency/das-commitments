using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.HashingService;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingApprenticeshipByUln
    {
        private Mock<ILog> _logger;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<ApprenticeshipSearchQuery>> _searchValidator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private Mock<IHashingService> _hashingService;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private ApprenticeshipsOrchestrator _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILog>();
            _mediator = new Mock<IMediator>();
            _searchValidator = new Mock<IValidator<ApprenticeshipSearchQuery>>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _hashingService = new Mock<IHashingService>();
            _commitmentMapper = new Mock<ICommitmentMapper>();

            _apprenticeshipMapper
              .Setup(o => o.MapToUlnResultView(It.IsAny<GetApprenticeshipsByUlnResponse>()))
              .Returns(new UlnSummaryViewModel())
              .Verifiable();

            _hashingService
                .Setup(o => o.DecodeValue(It.IsAny<string>()))
                .Returns(100);

            _hashingService
             .Setup(o => o.HashValue(It.IsAny<long>()))
             .Returns("ABCDE500");

            _logger.Setup(x => x.Trace(It.IsAny<string>()));
            _logger.Setup(x => x.Info(It.IsAny<string>()));

            _sut = new ApprenticeshipsOrchestrator(_logger.Object,
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _hashingService.Object,
                _commitmentMapper.Object);
        }

        [Test]
        public async Task GivenValidUlnShouldCallRequiredServices()
        {
            // Arrasnge
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                SearchTerm = "1000201219",
                SearchType = ApprenticeshipSearchType.SearchByUln
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsByUlnRequest>()))
            .ReturnsAsync(new GetApprenticeshipsByUlnResponse
            {
                TotalCount = 1,
                Apprenticeships = GetApprenticeships()
            }).Verifiable();

            var validationResult = new Mock<ValidationResult>();
            validationResult.SetupGet(x => x.IsValid).Returns(true);

            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult.Object)
                .Verifiable();

            _apprenticeshipMapper
                .Setup(o => o.MapToUlnResultView(It.IsAny<GetApprenticeshipsByUlnResponse>()))
                .Returns(new UlnSummaryViewModel())
                .Verifiable();

            // Act
            var result = await _sut.GetApprenticeshipsByUln(searchQuery);

            // Arrange
            _searchValidator.VerifyAll();
            _mediator.VerifyAll();
            _apprenticeshipMapper.VerifyAll();

        }

        [Test]
        public async Task GivenInvalidUlnShouldReturnResponseMessageAndNotCallSearchService()
        {
            // Arrasnge
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                SearchTerm = "000000001",
                SearchType = ApprenticeshipSearchType.SearchByUln
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsByUlnRequest>()))
            .ReturnsAsync(new GetApprenticeshipsByUlnResponse
            {
                TotalCount = 1,
                Apprenticeships = GetApprenticeships()
            });

            var validationFailures = new List<ValidationFailure>
                    {
                       new ValidationFailure("SearchTerm","Invalid Uln")
                    };

            var validationResult = new ValidationResult(validationFailures);


            _searchValidator.Setup(x => x.Validate(searchQuery))
                .Returns(validationResult)
                .Verifiable();

            // Act
            var result = await _sut.GetApprenticeshipsByUln(searchQuery);

            // Assert
            _searchValidator.VerifyAll();
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetApprenticeshipsByUlnRequest>()), Times.Never);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().HaveCount(1);

        }

        [Test]
        public async Task WhenNoUlnRecordIsFoundShouldReturnResponseMessages()
        {
            // Arrasnge
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery
            {
                SearchTerm = "1000201219",
                SearchType = ApprenticeshipSearchType.SearchByUln
            };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsByUlnRequest>()))
             .ReturnsAsync(new GetApprenticeshipsByUlnResponse
             {
                 TotalCount = 0,
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
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetApprenticeshipsByUlnRequest>()), Times.Once);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSummaryViewModel>();

            result.ReponseMessages.Should().NotBeNull();
            result.ReponseMessages.Should().HaveCount(1);

        }

        private List<Apprenticeship> GetApprenticeships()
        {
            return new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "Testoo1",
                    StartDate = new DateTime(2020,1,1)
                }
            };
        }

    }
}
