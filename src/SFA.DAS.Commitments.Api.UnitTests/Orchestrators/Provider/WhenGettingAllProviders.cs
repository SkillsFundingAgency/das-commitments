using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetProviders;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    public class WhenGettingAllProviders : ProviderOrchestratorTestBase
    {
        
        [Test]
        public async Task Then_The_Providers_Are_Returned_From_Mediator()
        {
            //Arrange
            var fixture = new Fixture();
            var getProvidersResponse = fixture.CreateMany<ProviderResponse>().ToList();
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetProvidersQuery>()))
                .ReturnsAsync(new GetProvidersQueryResponse
                {
                    Providers   = getProvidersResponse
                });

            //Act
            var actual = await Orchestrator.GetProviders();
            
            //Assert
            actual.Providers.ShouldBeEquivalentTo(getProvidersResponse);
        }
    }
}