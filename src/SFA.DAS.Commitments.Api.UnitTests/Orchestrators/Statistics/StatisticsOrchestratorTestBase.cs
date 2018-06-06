using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Statistics
{
    public class StatisticsOrchestratorTestBase
    {
        protected Mock<IMediator> MockMediator;
        protected StatisticsOrchestrator Orchestrator;

        [SetUp]
        public void Setup()
        {
            MockMediator = new Mock<IMediator>();
            Orchestrator = new StatisticsOrchestrator(MockMediator.Object, Mock.Of<ICommitmentsLogger>(),
                new StatisticsMapper());
        }
    }
}
