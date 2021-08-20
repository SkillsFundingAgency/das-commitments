using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipInfoServiceMapperTests
{
    [TestFixture]
    public class WhenIMapStandard
    {
        private ApprenticeshipInfoServiceMapper _mapper;
        private Standard _standard;

        [SetUp]
        public void Arrange()
        {
            _mapper = new ApprenticeshipInfoServiceMapper();

            _standard = new Standard
            {
                LarsCode = "1",
                Title = "TestTitle",
                Level = 1,
                MaxFunding = 1000, //this is to become redundant
                EffectiveFrom = new DateTime(2017, 05, 01),
                EffectiveTo = new DateTime(2020, 7, 31),
                FundingPeriods = new List<FundingPeriod>
                {
                    new FundingPeriod { EffectiveFrom = new DateTime(2017,05,01), EffectiveTo = new DateTime(2018, 12, 31), FundingCap = 5000 },
                    new FundingPeriod { EffectiveFrom = new DateTime(2019,01,01), EffectiveTo = new DateTime(2020, 7, 31), FundingCap = 2000 }
                }
            };
        }

        [Test]
        public void ThenTitleIsMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<Standard> { TestHelper.Clone(_standard) });

            //Assert
            var expectedTitle = $"{_standard.Title}, Level: {_standard.Level}";
            Assert.AreEqual(expectedTitle, result.Standards[0].Title);
        }

        [Test]
        public void ThenEffectiveFromIsMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<Standard> { TestHelper.Clone(_standard) });

            //Assert
            Assert.AreEqual(_standard.EffectiveFrom, result.Standards[0].EffectiveFrom);
        }

        [Test]
        public void ThenEffectiveToIsMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<Standard> { TestHelper.Clone(_standard) });

            //Assert
            Assert.AreEqual(_standard.EffectiveTo, result.Standards[0].EffectiveTo);
        }

        [Test]
        public void ThenFundingPeriodsAreMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<Standard> { TestHelper.Clone(_standard) });

            var comparer = new CompareLogic(new ComparisonConfig
            {
                IgnoreObjectTypes = true
            });

            Assert.IsTrue(comparer.Compare(result.Standards[0].FundingPeriods, _standard.FundingPeriods).AreEqual);
        }

        [Ignore("FAT2-294 - Not possible to have no funding periods")]
        public void ThenFundingPeriodsAreMappedCorrectlyWhenNull()
        {
            //Arrange
            _standard.FundingPeriods = null;

            //Act
            var result = _mapper.MapFrom(new List<Standard> { TestHelper.Clone(_standard) });

            //Assert
            Assert.IsNotNull(result.Standards[0].FundingPeriods);
            Assert.IsEmpty(result.Standards[0].FundingPeriods);
        }
    }
}