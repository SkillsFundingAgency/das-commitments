using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetProvider;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetProvider
{
    public class WhenHandlingGetProviderQuery
    {
        private Mock<IProviderRepository> _repository;
        private GetProviderQueryHandler _handler;
        private Provider _provider;
        private const long Ukprn = 3421;
    
        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _provider = fixture.Create<Provider>();
            _repository = new Mock<IProviderRepository>();
            _repository.Setup(x => x.GetProvider(Ukprn)).ReturnsAsync(_provider);
            _handler = new GetProviderQueryHandler(_repository.Object);
        }
        
        [Test]
        public async Task Then_The_Service_Is_Called_And_TrainingProgramme_Returned()
        {
            //Arrange
            var query = new GetProviderQuery
            {
                Ukprn = Ukprn
            };
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            actual.Provider.ShouldBeEquivalentTo(_provider);
        }
    }
}