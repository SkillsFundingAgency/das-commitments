using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetProviders;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetProviders
{
    public class WhenHandlingGetProvidersQuery
    {
        public class WhenHandlingGetProviderQuery
        {
            private Mock<IProviderRepository> _repository;
            private GetProvidersQueryHandler _handler;
            private List<Provider> _provider;
            
            [SetUp]
            public void Arrange()
            {
                var fixture = new Fixture();
                _provider = fixture.CreateMany<Provider>().ToList();
                _repository = new Mock<IProviderRepository>();
                _repository.Setup(x => x.GetProviders()).ReturnsAsync(_provider);
                _handler = new GetProvidersQueryHandler(_repository.Object);
            }
        
            [Test]
            public async Task Then_The_Service_Is_Called_And_Providers_Returned()
            {
                //Arrange
                var query = new GetProvidersQuery();
            
                //Act
                var actual = await _handler.Handle(query);
            
                //Assert
                actual.Providers.ShouldBeEquivalentTo(_provider);
            }
        }
    }
}