using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using System.Threading;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingApprenticeshipByUln
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

            _encodingService
                .Setup(o => o.Decode(It.IsAny<string>(), It.IsAny<EncodingType>()))
                .Returns(100);

            _encodingService
             .Setup(o => o.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()))
             .Returns("ABCDE500");

            _sut = new ApprenticeshipsOrchestrator(Mock.Of<ILogger<ApprenticeshipsOrchestrator>>(),
                _mediator.Object,
                _apprenticeshipMapper.Object,
                _searchValidator.Object,
                _encodingService.Object,
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

            _mediator.Setup(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetSupportApprenticeshipQueryResult
            {
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
            _mediator.Verify(x => x.Send(It.IsAny<GetSupportApprenticeshipQuery>(), CancellationToken.None), Times.Never);

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

        private List<SupportApprenticeshipDetails> GetApprenticeships()
        {
            return new List<SupportApprenticeshipDetails>
            {
                new SupportApprenticeshipDetails
                {
                    FirstName = "Testoo1",
                    StartDate = new DateTime(2020,1,1)
                }
            };
        }
    }
}