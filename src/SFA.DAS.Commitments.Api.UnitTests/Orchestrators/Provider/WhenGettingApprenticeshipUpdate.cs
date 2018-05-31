using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenGettingApprenticeshipUpdate : ProviderOrchestratorTestBase
    {
        [Test]
        public async Task ThereIsNoUpdate()
        {
            var result = await Orchestrator.GetPendingApprenticeshipUpdate(666, 999);
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetPendingApprenticeshipUpdateRequest>()))
                .ReturnsAsync((GetPendingApprenticeshipUpdateResponse)null);
            result.Should().BeNull();
        }

        [Test]
        public async Task ThereIsAnUpdate()
        {
            var update = new Fixture().Create<Domain.Entities.ApprenticeshipUpdate>();
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetPendingApprenticeshipUpdateRequest>()))
                .ReturnsAsync(new GetPendingApprenticeshipUpdateResponse
                                  {
                                      Data = update
                                  });

            var result = await Orchestrator.GetPendingApprenticeshipUpdate(666, 999);
            result.Should().NotBeNull();
            result.ShouldBeEquivalentTo(update, config => 
                config
                .ExcludingMissingMembers()
                .Excluding(m => m.ULN)
                .Excluding(m => m.EmployerRef)
                .Excluding(m => m.ProviderRef)
                .Excluding(m => m.UpdateOrigin)
                .Excluding(m => m.EffectiveFromDate)
                .Excluding(m => m.EffectiveToDate)
                );
        }
    }
}