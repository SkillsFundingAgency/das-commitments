using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
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
            var trainingProgramme = new TrainingProgramme
            {
                Name = _standard.Title,
                CourseCode = _standard.Id,
                EffectiveFrom = _standard.EffectiveFrom,
                EffectiveTo = _standard.EffectiveTo,
                ProgrammeType = int.TryParse(_standard.Id, out var result) ?  ProgrammeType.Standard : ProgrammeType.Framework,
                FundingPeriods = _standard.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingCap = x.FundingCap
                }).ToList()
            };
            var query = new GetTrainingProgrammeQuery
            {
                Id = TrainingProgrammeId
            };
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            actual.TrainingProgramme.ShouldBeEquivalentTo(trainingProgramme);
        }
    }
}