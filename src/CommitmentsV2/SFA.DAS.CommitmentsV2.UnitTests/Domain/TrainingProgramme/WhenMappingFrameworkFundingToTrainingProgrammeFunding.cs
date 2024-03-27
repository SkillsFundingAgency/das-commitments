using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.TrainingProgramme
{
    public class WhenMappingFrameworkFundingToTrainingProgrammeFunding
    {
        [Test, RecursiveMoqAutoData]
        public void Then_The_Fields_Are_Mapped(FrameworkFundingPeriod source)
        {
            //Act
            var actual =  TrainingProgrammeFundingPeriod.Map(source);
            
            //Assert
            actual.Should().BeEquivalentTo(source, options => options
                .Excluding(c=>c.Id)
                .Excluding(c=>c.Framework));
        }
    }
}