using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetTrainingProgramme;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetTrainingProgramme
{
    public class WhenHandlingGetTrainingProgrammeQuery
    {
        private Mock<IApprenticeshipInfoService> _service;
        private GetTrainingProgrammeQueryHandler _handler;
        private Standard _standard;
        private const string TrainingProgrammeId = "123-abc";
    
        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _standard = fixture.Create<Standard>();
            _service = new Mock<IApprenticeshipInfoService>();
            _service.Setup(x => x.GetTrainingProgram(TrainingProgrammeId)).ReturnsAsync(_standard);
            _handler = new GetTrainingProgrammeQueryHandler(_service.Object);
        }
        
        [Test]
        public async Task Then_The_Service_Is_Called_And_TrainingProgramme_Returned()
        {
            //Arrange
            var query = new GetTrainingProgrammeQuery
            {
                Id = TrainingProgrammeId
            };
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            actual.TrainingProgramme.ShouldBeEquivalentTo(_standard);
        }
    }
}