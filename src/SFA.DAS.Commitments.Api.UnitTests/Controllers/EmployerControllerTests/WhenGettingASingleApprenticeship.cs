﻿using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    public class WhenGettingASingleApprenticeship
    {
        private const long TestProviderId = 1L;
        private const long TestApprenticeshipId = 3L;
        private Mock<IMediator> _mockMediator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private EmployerController _controller;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _commitmentMapper = new Mock<ICommitmentMapper>();
            _apprenticeshipMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.Apprenticeship>(), It.IsAny<CallerType>()))
                .Returns(new Apprenticeship.Apprenticeship());

            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(Mock.Of<ICurrentDateTime>()), new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())), _apprenticeshipMapper.Object, _commitmentMapper.Object);

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(
                _mockMediator.Object,
                Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenReturnsASingleCommitment(GetApprenticeshipResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId) as OkNegotiatedContentResult<Apprenticeship.Apprenticeship>;

            result.Content.Should().NotBeNull();
            _apprenticeshipMapper.Verify(
                x => x.MapFrom(It.IsAny<Domain.Entities.Apprenticeship>(), It.IsAny<CallerType>()), Times.Once);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentIdApprenticeshipIdProviderId()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse());

            var result = await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetApprenticeshipRequest>(arg => arg.ApprenticeshipId == TestApprenticeshipId && arg.Caller.CallerType == CallerType.Employer && arg.Caller.Id == TestProviderId)));
        }

        [Test]
        public void ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            const string errorMessage = "Error message";

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ThrowsAsync(new ValidationException(errorMessage));

            var validationException = Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId));

            Assert.That(validationException.Message, Is.EqualTo(errorMessage));
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse { Data = null });

            _apprenticeshipMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.Apprenticeship>(), It.IsAny<CallerType>()))
                .Returns(() => null);

            var result = await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}