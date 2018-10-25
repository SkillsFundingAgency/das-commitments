using System.Collections.Generic;
using System.Linq;

using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    public class ProviderOrchestratorTestBase
    {
        protected Mock<IMediator> MockMediator;
        protected ProviderOrchestrator Orchestrator;
        protected Mock<FacetMapper> MockFacetMapper;
        protected Mock<ApprenticeshipFilterService> MockApprenticeshipFilter;
        protected Mock<IApprovedApprenticeshipMapper> MockApprovedApprenticeshipMapper;

        [SetUp]
        public virtual void SetUp()
        {
            MockMediator = new Mock<IMediator>();
            MockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());
            MockApprenticeshipFilter = new Mock<ApprenticeshipFilterService>(MockFacetMapper.Object);
            MockApprovedApprenticeshipMapper = new Mock<IApprovedApprenticeshipMapper>();

            Orchestrator = new ProviderOrchestrator(
                MockMediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                MockFacetMapper.Object,
                MockApprenticeshipFilter.Object,
                new ApprenticeshipMapper(),
                Mock.Of<ICommitmentMapper>(),
                MockApprovedApprenticeshipMapper.Object);

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

            MockApprenticeshipFilter.Setup(m =>
               m.Filter(It.IsAny<IList<Types.Apprenticeship.Apprenticeship>>(), It.IsAny<ApprenticeshipSearchQuery>(), Originator.Provider))
               .Returns<IList<Types.Apprenticeship.Apprenticeship>, ApprenticeshipSearchQuery, Originator>((aps, q, o) => new FilterResult(100, aps.ToList(), 1, 25));
        }
    }
}