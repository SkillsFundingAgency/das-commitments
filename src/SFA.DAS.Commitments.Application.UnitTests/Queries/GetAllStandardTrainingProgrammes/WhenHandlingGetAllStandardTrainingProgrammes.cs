using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetAllStandardTrainingProgrammes
{
    public class WhenHandlingGetAllStandardTrainingProgrammes
    {
        private Mock<IApprenticeshipInfoService> _service;
        private GetAllStandardTrainingProgrammesQueryHandler _handler;
        private StandardsView _standard;
    
        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _standard = fixture.Create<StandardsView>();
            _service = new Mock<IApprenticeshipInfoService>();
            _service.Setup(x => x.GetStandards(false)).ReturnsAsync(_standard);
            _handler = new GetAllStandardTrainingProgrammesQueryHandler(_service.Object);
        }
        
        [Test]
        public async Task Then_The_Service_Is_Called_And_TrainingProgramme_Returned()
        {
            //Arrange
            var query = new GetAllStandardTrainingProgrammesQuery();
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            actual.TrainingProgrammes.ShouldBeEquivalentTo(_standard.Standards.OrderBy(c=>c.Title));
        }
    }
}