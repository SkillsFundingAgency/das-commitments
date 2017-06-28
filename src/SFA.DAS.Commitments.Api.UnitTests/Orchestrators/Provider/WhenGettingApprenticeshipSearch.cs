using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators;
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
                _mockApprenticeshipFilter.Object);
        }

        [Test]
        public async Task ShouldFilterListFromNotApproved()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                                  {
                                      Data = new List<Apprenticeship>
                                                 {
                                                     new Apprenticeship { PaymentStatus = PaymentStatus.Active },
                                                     new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval },
                                                     new Apprenticeship { PaymentStatus = PaymentStatus.Paused }
                                                 }
                                  });

            // Return same IList<Apprenticeship> as input.
            _mockApprenticeshipFilter.Setup(m => 
                m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider))
                .Returns<IList<Apprenticeship>, ApprenticeshipSearchQuery,Originator>((aps, q, o) => aps);

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            result.Apprenticeships.Count().Should().Be(2);
        }

        [Test]
        public async Task ShouldCallGetFacetAndMapper()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse { Data = new List<Apprenticeship>() });

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            _mockMediator.Verify(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()), Times.Once);
            _mockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);
            _mockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);

            result.Apprenticeships.Count().Should().Be(0);
        }

        [TestCase(1, 100, 10, 1, Description = "Returns first page number if first page number passed in")]
        [TestCase(14, 100, 10, 1, Description = "Returns first page number if page is not within range of total pages")]
        [TestCase(10, 100, 10, 10, Description = "Returns page number if page is not within range of total pages")]
        [TestCase(0, 100, 10, 1, Description = "Returns first page if page is not set (0)")]
        public async Task ShouldReturnThePageNumber(int requestedPageNumber, int totalApprenticeships, int requestedPageSize, int expectedPageNumber)
        {
            var apprenticeships = CreateApprenticeships(totalApprenticeships);

            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse { Data = apprenticeships });

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery { PageNumber = requestedPageNumber, PageSize = requestedPageSize });

            result.PageNumber.Should().Be(expectedPageNumber);
        }

        [TestCase(5, 5, Description = "Returns page size from query if > 0")]
        [TestCase(0, 25, Description = "Defaults page size to 25 if the page size is not set (0)")]
        public async Task ShouldReturnThePageSize(int requestedPageSize, int expectedPageSize)
        {
            var apprenticeships = CreateApprenticeships(20);

            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse { Data = apprenticeships });

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery { PageNumber = 1, PageSize = requestedPageSize });

            result.PageSize.Should().Be(expectedPageSize);
        }

        [Test]
        public async Task ShouldDefaultPagingValuesIfNotSet()
        {
            var apprenticeships = CreateApprenticeships(20);

            _mockMediator.Setup(m => m.SendAsync(It.Is<GetApprenticeshipsRequest>(x => x.PageNumber == 1 && x.PageSize == 25)))
                .ReturnsAsync(new GetApprenticeshipsResponse { Data = apprenticeships });

            var result = await _orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery { PageNumber = 0, PageSize = 0 });

            _mockMediator.Verify();
        }

        private static List<Apprenticeship> CreateApprenticeships(int totalApprenticeships)
        {
            var apprenticeships = new List<Apprenticeship>(totalApprenticeships);

            for (int i = 0; i < totalApprenticeships - 1; i++)
            {
                apprenticeships.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Active });
            }

            return apprenticeships;
        }
    }
}
