using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class TrainingProgrammeLookupTests
    {
        [Test, MoqAutoData]
        public async Task Then_If_There_Is_No_Code_Null_Is_Returned(
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Act
            var actual = await service.GetTrainingProgramme("");

            //Assert
            actual.Should().BeNull();
        }
        
        [Test, RecursiveMoqAutoData]
        public async Task Then_If_The_Course_Code_Is_Numeric_Then_Standards_Are_Searched(
            Standard standard,
            List<Standard> standards,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service
            )
        {
            //Arrange
            standards.Add(standard);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            //Act
            var actual = await service.GetTrainingProgramme(standard.LarsCode.ToString());
            
            //Assert
            actual.CourseCode.Should().Be(standard.LarsCode.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level}");
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
            dbContext.Verify(x=>x.Frameworks.FindAsync(It.IsAny<int>()), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_It_Is_Not_Numeric_Then_Frameworks_Are_Searched(
            Framework framework,
            List<Framework> frameworks,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service
            )
        {
            //Arrange
            frameworks.Add(framework);
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(frameworks);
            
            //Act
            var actual = await service.GetTrainingProgramme(framework.Id);
            
            //Assert
            actual.CourseCode.Should().Be(framework.Id);
            actual.Name.Should().Be($"{framework.Title}, Level: {framework.Level} (Framework)");
            actual.EffectiveFrom.Should().Be(framework.EffectiveFrom);
            actual.EffectiveTo.Should().Be(framework.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Framework);
            dbContext.Verify(x=>x.Standards.FindAsync(It.IsAny<int>()), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public void Then_If_Find_Standard_Returns_Null_An_Exception_Is_Thrown(
            int standardCode,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            dbContext.Setup(x => x.Standards).ReturnsDbSet(new List<Standard>());
            
            //Act Assert
            Assert.ThrowsAsync<Exception>(()=> service.GetTrainingProgramme(standardCode.ToString()),$"The course code {standardCode} was not found");
        }
        
        [Test, RecursiveMoqAutoData]
        public void Then_If_Find_Framework_Returns_Null_An_Exception_Is_Thrown(
            string frameworkId,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(new List<Framework>());
            
            //Act Assert
            Assert.ThrowsAsync<Exception>(()=> service.GetTrainingProgramme(frameworkId),$"The course code {frameworkId} was not found");
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards_And_Frameworks_When_Getting_All(
            List<Framework> frameworks,
            List<Standard> standards,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            standards.ForEach(s => s.IsLatestVersion = true);
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(frameworks);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);
            
            //Act
            var actual = (await service.GetAll()).ToList();
            
            //Assert
            actual.Count.Should().Be(frameworks.Count + standards.Count);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards(
            List<Standard> standards,
            [Frozen]Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            standards.ForEach(s => s.IsLatestVersion = true);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);
            
            //Act
            var actual = (await service.GetAllStandards()).ToList();
            
            //Assert
            actual.Count.Should().Be(standards.Count);
        }
    }
}