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

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingApprenticeshipSearch
    {
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _orchestrator;

        private Mock<FacetMapper> _mockFacetMapper;

        private Mock<ApprenticeshipFilterService> _mockApprenticeshipFilter;

        [SetUp]
        public void SetUp()
        {
            _mockMediator = new Mock<IMediator>();
            _mockFacetMapper = new Mock<FacetMapper>();
            _mockApprenticeshipFilter = new Mock<ApprenticeshipFilterService>(_mockFacetMapper.Object);
            _orchestrator = new EmployerOrchestrator(
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
                m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer))
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
            _mockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer), Times.Once);
            _mockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer), Times.Once);

            result.Apprenticeships.Count().Should().Be(0);
        }
    }
}
