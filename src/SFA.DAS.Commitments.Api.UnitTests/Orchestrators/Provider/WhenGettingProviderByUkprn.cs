using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetProvider;
using It = Moq.It;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    public class WhenGettingProviderByUkprn : ProviderOrchestratorTestBase
    {
        
        [Test]
        public async Task Then_The_Provider_Is_Returned_From_Mediator()
        {
            //Arrange
            var fixture = new Fixture();
            var ukprn = fixture.Create<long>();
            var provider = fixture.Create<ProviderResponse>();
            MockMediator.Setup(m => m.SendAsync(It.Is<GetProviderQuery>(c=>c.Ukprn.Equals(ukprn))))
                .ReturnsAsync(new GetProviderQueryResponse()
                {
                    Provider = provider
                });

            //Act
            var actual = await Orchestrator.GetProvider(ukprn);
            
            //Assert
            actual.Provider.ShouldBeEquivalentTo(provider);
        }
    }
}