using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipInfoServiceMapperTests
{
    [TestFixture]
    public class WhenIMapFramework
    {
        private ApprenticeshipInfoServiceMapper _mapper;
        private FrameworkSummary _framework;

        [SetUp]
        public void Arrange()
        {
            _mapper = new ApprenticeshipInfoServiceMapper();

            _framework = new FrameworkSummary
            {
                Id = "1",
                Title = "TestTitle",
                FrameworkName = "TestFrameworkName",
                PathwayName = "TestPathwayName",
                Level = 1,
                CurrentFundingCap = 1000, //this is to become redundant
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
            var result = _mapper.MapFrom(new List<FrameworkSummary> { CopyOf(_framework) });

            //Assert
            var expectedTitle = $"{_framework.Title}, Level: {_framework.Level}";
            Assert.AreEqual(expectedTitle, result.Frameworks[0].Title);
        }

        [Test]
        public void ThenEffectiveFromIsMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<FrameworkSummary> { CopyOf(_framework) });

            //Assert
            Assert.AreEqual(_framework.EffectiveFrom, result.Frameworks[0].EffectiveFrom);
        }

        [Test]
        public void ThenEffectiveToIsMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<FrameworkSummary> { CopyOf(_framework) });

            //Assert
            Assert.AreEqual(_framework.EffectiveFrom, result.Frameworks[0].EffectiveFrom);
        }


        [Test]
        public void ThenFundingPeriodsAreMappedCorrectly()
        {
            //Act
            var result = _mapper.MapFrom(new List<FrameworkSummary> { CopyOf(_framework) });

            //Assert
            var comparer = new CompareLogic(new ComparisonConfig
            {
                IgnoreObjectTypes = true
            });

            Assert.IsTrue(comparer.Compare(result.Frameworks[0].FundingPeriods, _framework.FundingPeriods).AreEqual);
        }

        [Test]
        public void ThenFundingPeriodsAreMappedCorrectlyWhenNull()
        {
            //Arrange
            _framework.FundingPeriods = null;

            //Act
            var result = _mapper.MapFrom(new List<FrameworkSummary> { CopyOf(_framework) });

            //Assert
            Assert.IsNotNull(result.Frameworks[0].FundingPeriods);
            Assert.IsEmpty(result.Frameworks[0].FundingPeriods);
        }

        private static FrameworkSummary CopyOf(FrameworkSummary source)
        {
            //copy the payload to guard against handler modifications
            return JsonConvert.DeserializeObject<FrameworkSummary>(JsonConvert.SerializeObject(source));
        }
    }
}
