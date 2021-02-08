using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetAllTrainingProgrammes
{
    public class WhenHandlingGetAllTrainingProgrammesQuery
    {
        private Mock<IApprenticeshipInfoService> _service;
        private GetAllTrainingProgrammesQueryHandler _handler;
        private StandardsView _standard;
        private FrameworksView _frameworks;
    
        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _standard = fixture.Create<StandardsView>();
            _frameworks = fixture.Create<FrameworksView>();
            _service = new Mock<IApprenticeshipInfoService>();
            _service.Setup(x => x.GetStandards(false)).ReturnsAsync(_standard);
            _service.Setup(x => x.GetFrameworks(false)).ReturnsAsync(_frameworks);
            _handler = new GetAllTrainingProgrammesQueryHandler(_service.Object);
        }
        
        [Test]
        public async Task Then_The_Service_Is_Called_And_TrainingProgrammes_Returned()
        {
            //Arrange
            var query = new GetAllTrainingProgrammesQuery();
            
            //Act
            var actual = await _handler.Handle(query);
            
            //Assert
            var expectedList = new List<TrainingProgramme>();
            expectedList.AddRange(_standard.Standards.Select(c=> new TrainingProgramme
            {
                Name = c.Title,
                CourseCode = c.Id,
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
            expectedList.AddRange(_frameworks.Frameworks.Select(c=> new TrainingProgramme
            {
                Name = c.Title,
                CourseCode = c.Id,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = ProgrammeType.Framework,
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