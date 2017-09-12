using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingApprenticeshipSearch : EmployerOrchestratorTestBase
    {

        [Test]
        public async Task ShouldFilterListFromNotApproved()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                                  {
                                      Apprenticeships = new List<Domain.Entities.Apprenticeship>
                                                 {
                                                     new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active },
                                                     new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.PendingApproval },
                                                     new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Paused }
                                                 }
                                  });

            // Return same IList<Apprenticeship> as input.
            MockApprenticeshipFilter.Setup(m => 
                m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer))
                .Returns<IList<Apprenticeship>, ApprenticeshipSearchQuery,Originator>((aps, q, o) => new FilterResult(100, aps.ToList(), 1, 25));

            var result = await Orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            result.Apprenticeships.Count().Should().Be(2);
        }

        [Test]
        public async Task ShouldCallGetFacetAndMapper()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse { Apprenticeships = new List<Domain.Entities.Apprenticeship>() });
            MockApprenticeshipFilter.Setup(m =>
                m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer))
                .Returns<IList<Apprenticeship>, ApprenticeshipSearchQuery, Originator>((aps, q, o) => new FilterResult(100, aps.ToList(), 1, 25));

            var result = await Orchestrator.GetApprenticeships(1L, new ApprenticeshipSearchQuery());

            MockMediator.Verify(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()), Times.Once);
            MockFacetMapper.Verify(m => m.BuildFacets(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer), Times.Once);
            MockApprenticeshipFilter.Verify(m => m.Filter(It.IsAny<IList<Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Employer), Times.Once);

            result.Apprenticeships.Count().Should().Be(0);
        }
    }
}
