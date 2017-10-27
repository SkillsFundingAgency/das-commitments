using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;

using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenGettingApprenticeshipSearch : ProviderOrchestratorTestBase
    {

        [Test]
        public async Task ShouldFilterListFromNotApproved()
        {
            var result = await Orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            result.Apprenticeships.Count().Should().Be(2);
        }

        [Test]
        public async Task ShouldCallGetFacetAndMapper()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Apprenticeships = new List<Domain.Entities.Apprenticeship>()
                });

            var result = await Orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            MockMediator.Verify(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()), Times.Once);
            MockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Types.Apprenticeship.Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);
            MockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Types.Apprenticeship.Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);

            result.Apprenticeships.Count().Should().Be(0);
        }

        [Test]
        public async Task ShouldRemovePendingApprovalFromTotalCount()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Apprenticeships = new List<Domain.Entities.Apprenticeship>
                        {
                            new Domain.Entities.Apprenticeship { PaymentStatus = PaymentStatus.Active },
                            new Domain.Entities.Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval }
                        },
                    TotalCount = 50
                });

            var result = await Orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            MockMediator.Verify(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()), Times.Once);
            MockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Types.Apprenticeship.Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);
            MockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Types.Apprenticeship.Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider), Times.Once);

            result.Apprenticeships.Count().Should().Be(1);
            result.TotalApprenticeshipsBeforeFilter.Should().Be(49);
        }
    }
}
