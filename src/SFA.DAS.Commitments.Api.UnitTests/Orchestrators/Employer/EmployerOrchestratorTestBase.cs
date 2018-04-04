using MediatR;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    public class EmployerOrchestratorTestBase
    {
        protected Mock<IMediator> MockMediator;
        protected EmployerOrchestrator Orchestrator;
        protected Mock<FacetMapper> MockFacetMapper;
        protected Mock<ApprenticeshipFilterService> MockApprenticeshipFilter;
        protected Mock<ITransferRequestMapper> MockTransferRequestMapper;

        [SetUp]
        public void SetUp()
        {
            MockMediator = new Mock<IMediator>();
            MockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());
            MockApprenticeshipFilter = new Mock<ApprenticeshipFilterService>(MockFacetMapper.Object);
            MockTransferRequestMapper = new Mock<ITransferRequestMapper>();
            Orchestrator = new EmployerOrchestrator(
                MockMediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                MockFacetMapper.Object,
                MockApprenticeshipFilter.Object,
                new ApprenticeshipMapper(),
                Mock.Of<ICommitmentMapper>(),
                MockTransferRequestMapper.Object);
        }
    }
}