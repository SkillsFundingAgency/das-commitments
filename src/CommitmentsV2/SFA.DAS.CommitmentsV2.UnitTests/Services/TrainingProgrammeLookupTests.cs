using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
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
        private ProviderCommitmentsDbContext _db;

        [SetUp]
        public void Arrange()
        {
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);
            
            
        }
        
        [Test]
        public async Task Then_If_There_Is_No_Code_Null_Is_Returned()
        {
            //Arrange
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act
            var actual = await service.GetTrainingProgramme("");

            //Assert
            actual.Should().BeNull();
        }
        
        [Test, RecursiveMoqAutoData]
        public async Task Then_If_The_Course_Code_Is_Numeric_Then_Standards_Are_Searched(
            Standard standard,
            List<Standard> standards,
            StandardFundingPeriod standardFundingPeriod
        )
        {
            //Arrange
            standards.Add(standard);
            foreach (var std in standards)
            {
                std.FundingPeriods = new List<StandardFundingPeriod>
                {
                    new StandardFundingPeriod
                    {
                        Id = std.Id,
                        EffectiveFrom = standardFundingPeriod.EffectiveFrom,
                        EffectiveTo = standardFundingPeriod.EffectiveTo,
                        FundingCap = standardFundingPeriod.FundingCap,
                    }
                };
            }
            await _db.Standards.AddRangeAsync(standards);
            await _db.SaveChangesAsync();
            
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            

            //Act
            var actual = await service.GetTrainingProgramme(standard.Id.ToString());
            
            //Assert
            actual.CourseCode.Should().Be(standard.Id.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level} (Standard)");
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_It_Is_Not_Numeric_Then_Frameworks_Are_Searched(
            FrameworkFundingPeriod frameworkFundingPeriod,
            Framework framework,
            List<Framework> frameworks
            )
        {
            //Arrange
            frameworks.Add(framework);
            foreach (var frk in frameworks)
            {
                frk.FundingPeriods = new List<FrameworkFundingPeriod>
                {
                    new FrameworkFundingPeriod
                    {
                        Id = frk.Id,
                        EffectiveFrom = frameworkFundingPeriod.EffectiveFrom,
                        EffectiveTo = frameworkFundingPeriod.EffectiveTo,
                        FundingCap = frameworkFundingPeriod.FundingCap,
                    }
                };
            }
            await _db.Frameworks.AddRangeAsync(frameworks);
            await _db.SaveChangesAsync();
            
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act
            var actual = await service.GetTrainingProgramme(framework.Id);
            
            //Assert
            actual.CourseCode.Should().Be(framework.Id);
            actual.Name.Should().Be($"{framework.Title}, Level: {framework.Level}");
            actual.EffectiveFrom.Should().Be(framework.EffectiveFrom);
            actual.EffectiveTo.Should().Be(framework.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Framework);
        }

        [Test, AutoData]
        public void Then_If_Find_Standard_Returns_Null_An_Exception_Is_Thrown(int standardCode)
        {
            //Arrange
            _db.Standards.RemoveRange(_db.Standards);
            _db.SaveChanges();
            
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act Assert
            Assert.ThrowsAsync<Exception>(()=> service.GetTrainingProgramme(standardCode.ToString()),$"The course code {standardCode} was not found");
        }
        
        [Test, AutoData]
        public void Then_If_Find_Framework_Returns_Null_An_Exception_Is_Thrown(string frameworkId)
        {
            //Arrange
            _db.Frameworks.RemoveRange(_db.Frameworks);
            _db.SaveChanges();
            
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act Assert
            Assert.ThrowsAsync<Exception>(()=> service.GetTrainingProgramme(frameworkId),$"The course code {frameworkId} was not found");
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards_And_Frameworks_When_Getting_All(
            List<Framework> frameworks,
            List<Standard> standards,
            FrameworkFundingPeriod frameworkFundingPeriod,
            StandardFundingPeriod standardFundingPeriod)
        {
            //Arrange
            foreach (var frk in frameworks)
            {
                frk.FundingPeriods = new List<FrameworkFundingPeriod>
                {
                    new FrameworkFundingPeriod
                    {
                        Id = frk.Id,
                        EffectiveFrom = frameworkFundingPeriod.EffectiveFrom,
                        EffectiveTo = frameworkFundingPeriod.EffectiveTo,
                        FundingCap = frameworkFundingPeriod.FundingCap,
                    }
                };
            }
            await _db.Frameworks.AddRangeAsync(frameworks);
            foreach (var std in standards)
            {
                std.FundingPeriods = new List<StandardFundingPeriod>
                {
                    new StandardFundingPeriod
                    {
                        Id = std.Id,
                        EffectiveFrom = standardFundingPeriod.EffectiveFrom,
                        EffectiveTo = standardFundingPeriod.EffectiveTo,
                        FundingCap = standardFundingPeriod.FundingCap,
                    }
                };
            }
            await _db.Standards.AddRangeAsync(standards);
            await _db.SaveChangesAsync();
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act
            var actual = (await service.GetAll()).ToList();
            
            //Assert
            actual.Count.Should().Be(frameworks.Count + standards.Count);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards(
            StandardFundingPeriod standardFundingPeriod,
            List<Standard> standards)
        {
            //Arrange
            foreach (var std in standards)
            {
                std.FundingPeriods = new List<StandardFundingPeriod>
                {
                    new StandardFundingPeriod
                    {
                        Id = std.Id,
                        EffectiveFrom = standardFundingPeriod.EffectiveFrom,
                        EffectiveTo = standardFundingPeriod.EffectiveTo,
                        FundingCap = standardFundingPeriod.FundingCap,
                    }
                };
            }
            await _db.Standards.AddRangeAsync(standards);
            await _db.SaveChangesAsync();
            var service = new TrainingProgrammeLookup(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            
            //Act
            var actual = (await service.GetAllStandards()).ToList();
            
            //Assert
            actual.Count.Should().Be(standards.Count);
        }
    }
}