using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
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
        public async Task Then_The_Service_Is_Called_And_TrainingProgramme_Standards_Returned()
        {
            //Arrange
            var query = new GetAllStandardTrainingProgrammesQuery();
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            var expectedList = new List<TrainingProgramme>();
            expectedList.AddRange(_standard.Standards.Select(c=> new TrainingProgramme
            {
                Name = c.Title,
                CourseCode = c.LarsCode,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = ProgrammeType.Standard,
                FundingPeriods = c.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingCap = x.FundingCap
                }).ToList()
            }));
            actual.TrainingProgrammes.ShouldBeEquivalentTo(expectedList.OrderBy(c=>c.Name));
        }
    }
}