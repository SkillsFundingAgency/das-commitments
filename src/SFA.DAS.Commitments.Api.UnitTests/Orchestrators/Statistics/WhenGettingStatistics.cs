using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetStatistics;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Statistics
{
    [TestFixture]
    public class WhenGettingStatistics : StatisticsOrchestratorTestBase
    {
        [Test]
        public async Task ShouldReturnStatistics()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetStatisticsRequest>())).ReturnsAsync(
                new GetStatisticsResponse
                {
                    Data = new Domain.Entities.Statistics
                    {
                        TotalApprenticeships = 100,
                        TotalCohorts = 25,
                        ActiveApprenticeships = 23
                    }
                });

            var result = await Orchestrator.GetStatistics();

            result.TotalApprenticeships.Should().Be(100);
            result.ActiveApprenticeships.Should().Be(23);
            result.TotalCohorts.Should().Be(25);
        }
    }
}
