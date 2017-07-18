using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenGettingApprenticeshipSearch
    {
        private Mock<IMediator> _mockMediator;
        private ProviderOrchestrator _orchestrator;

        private Mock<FacetMapper> _mockFacetMapper;

        private Mock<ApprenticeshipFilterService> _mockApprenticeshipFilter;

        [SetUp]
        public void SetUp()
        {
            _mockMediator = new Mock<IMediator>();
            _mockFacetMapper = new Mock<FacetMapper>();
            _mockApprenticeshipFilter = new Mock<ApprenticeshipFilterService>(_mockFacetMapper.Object);
            _orchestrator = new ProviderOrchestrator(
                _mockMediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                _mockFacetMapper.Object,
                _mockApprenticeshipFilter.Object,
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentMapper>());

            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Data = new List<Domain.Entities.Apprenticeship>
                            {
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active },
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.PendingApproval },
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Paused }
                            }
                });

            _mockApprenticeshipFilter.Setup(m =>
               m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider))
               .Returns<IList<Apprenticeship>, ApprenticeshipSearchQuery, Originator>((aps, q, o) => new FilterResult(100, aps.ToList(), 1, 25));
        }

        [Test]
        public async Task ShouldFilterListFromNotApproved()
        {
            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            result.Apprenticeships.Count().Should().Be(2);
        }

        [Test]
        public async Task ShouldCallGetFacetAndMapper()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Data = new List<Domain.Entities.Apprenticeship>()
                });

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            _mockMediator.Verify(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()), Times.Once);
            _mockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);
            _mockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);

            result.Apprenticeships.Count().Should().Be(0);
        }
    }
}
