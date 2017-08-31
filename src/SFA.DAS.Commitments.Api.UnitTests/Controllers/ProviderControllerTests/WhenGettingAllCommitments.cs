using System.Collections.Generic;
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
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenIGetAllProviderCommitments
    {
        private Mock<IMediator> _mockMediator;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private ProviderController _controller;
        private ProviderOrchestrator _providerOrchestrator;
        private Mock<FacetMapper> _mockFacetMapper;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _commitmentMapper = new Mock<ICommitmentMapper>();
            _commitmentMapper.Setup(x => x.MapFrom(It.IsAny<IEnumerable<CommitmentSummary>>(), It.IsAny<CallerType>()))
                .Returns(() => new List<CommitmentListItem>());

            _mockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());

            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockFacetMapper.Object,
                new ApprenticeshipFilterService(_mockFacetMapper.Object),
                Mock.Of<IApprenticeshipMapper>(),
                _commitmentMapper.Object);

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitments(1235L) as OkNegotiatedContentResult<IEnumerable<CommitmentListItem>>;

            result.Should().NotBeNull();
            _commitmentMapper.Verify(x => x.MapFrom(It.IsAny<IEnumerable<CommitmentSummary>>(), It.IsAny<CallerType>()));
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(new GetCommitmentsResponse());

            var result = await _controller.GetCommitments(testProviderId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentsRequest>(arg => arg.Caller.CallerType == CallerType.Provider && arg.Caller.Id == testProviderId)));
        }

        [Test]
        public void ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCommitments(1L));
        }
    }
}
