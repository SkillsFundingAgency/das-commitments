using System;
using System.Threading.Tasks;
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
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public sealed class WhenGettingASingleApprenticeship
    {
        private const long TestProviderId = 1;
        private const long TestApprenticeshipId = 3L;
        private Mock<IMediator> _mockMediator;
        private Mock<IApprenticeshipMapper> _mockApprenticeshipMapper;
        private ProviderController _controller;
        private ProviderOrchestrator _providerOrchestrator;

        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _mockApprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _mockApprenticeshipMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.Apprenticeship>(), It.IsAny<CallerType>()))
                .Returns(new Apprenticeship());

            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                new FacetMapper(Mock.Of<ICurrentDateTime>()),
                new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())),
                _mockApprenticeshipMapper.Object,
                Mock.Of<ICommitmentMapper>());

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenReturnsASingleMappedApprenticeship(GetApprenticeshipResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId) as OkNegotiatedContentResult<Apprenticeship>;

            result.Content.Should().NotBeNull();

            _mockApprenticeshipMapper.Verify(x => x.MapFrom(It.IsAny<Domain.Entities.Apprenticeship>(), It.IsAny<CallerType>())
                ,Times.Once);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentIdApprenticeshipIdProviderId()
        {
            const long testProviderId = 2222L;
            const long testApprenticeshipId = 4321L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse());

            var result = await _controller.GetApprenticeship(testProviderId, testApprenticeshipId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetApprenticeshipRequest>(arg => arg.ApprenticeshipId == testApprenticeshipId && arg.Caller.CallerType == CallerType.Provider && arg.Caller.Id == testProviderId)));
        }

        [TestCase]
        public void ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId));
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse {Data = null});

            var result = await _controller.GetApprenticeship(TestProviderId, TestApprenticeshipId);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
