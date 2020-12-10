using System;
using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.TrainingProgramme
{
    [TestFixture]
    public class WhenDeterminingFundingCap
    {
        private CommitmentsV2.Domain.Entities.TrainingProgramme _course;

        [SetUp]
        public void Arrange()
        {
            
            
            var fundingPeriods = new List<StandardFundingPeriod>
            {
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,03,01),
                    EffectiveTo = new DateTime(2018,07,31),
                    FundingCap = 5000
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,01),
                    EffectiveTo = null,
                    FundingCap = 2000
                }
            };
            _course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1", "Test", ProgrammeType.Standard,
                new DateTime(2018, 03, 01), new DateTime(2019, 03, 31), new List<IFundingPeriod>(fundingPeriods));
        }

        [TestCase("2018-05-15", 5000, Description = "Within first funding band")]
        [TestCase("2018-09-15", 2000, Description = "Within second funding band")]
        [TestCase("2018-01-01", 0, Description = "Before course start")]
        [TestCase("2019-06-01", 0, Description = "After course end")]
        public void ThenTheApplicableFundingPeriodIsUsed(DateTime effectiveDate, int expectCap)
        {
            //Act
            var result = _course.FundingCapOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectCap, result);
        }

        [TestCase("2020-01-01", 0, Description = "Before funding band")]
        [TestCase("2020-02-01", 1, Description = "Before funding band but in same month")]
        [TestCase("2020-02-20", 1, Description = "Within open-ended funding band first day in same month")]
        [TestCase("2020-02-21", 1, Description = "Within open-ended funding band in same month")]
        [TestCase("2020-03-01", 1, Description = "Within open-ended funding band")]
        public void AndOnlyFundingPeriodHasEffectiveFromNotFirstOfMonthAndEffectiveToOpenEndedThenTheApplicableFundingPeriodIsUsed(DateTime effectiveDate, int expectCap)
        {
            var courseAndFundingBandStart = new DateTime(2020, 2, 20);
            var courseAndFundingBandEnd = (DateTime?)null;
            var fundingPeriods = new List<StandardFundingPeriod>
            {
                new StandardFundingPeriod
                {
                    EffectiveFrom = courseAndFundingBandStart,
                    EffectiveTo = courseAndFundingBandEnd,
                    FundingCap = 1
                }
            };

            _course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1", "Test", ProgrammeType.Standard,
                courseAndFundingBandStart, courseAndFundingBandEnd, new List<IFundingPeriod>(fundingPeriods));


            //Act
            var result = _course.FundingCapOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectCap, result);
        }

        [TestCase("2018-07-01", 1, Description = "Within first open-start funding band")]
        [TestCase("2018-07-31", 1, Description = "At end of first open-start funding band")]
        [TestCase("2018-08-01", 2, Description = "Start of second funding band")]
        [TestCase("2020-01-01", 2, Description = "Within second open-ended funding band")]
        public void AndFirstFundingPeriodHasNUllEffectiveFromThenTheApplicableFundingPeriodIsUsed(DateTime effectiveDate, int expectCap)
        {
            var courseAndFundingBandStart = (DateTime?)null;
            var courseAndFundingBandEnd = (DateTime?)null;
            var fundingPeriods = new List<StandardFundingPeriod>
            {
                new StandardFundingPeriod
                {
                    EffectiveFrom = courseAndFundingBandStart,
                    EffectiveTo = new DateTime(2018,07,31),
                    FundingCap = 1
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,01),
                    EffectiveTo = courseAndFundingBandEnd,
                    FundingCap = 2
                }
            };
            _course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1", "Test", ProgrammeType.Standard,
                courseAndFundingBandStart, courseAndFundingBandEnd, new List<IFundingPeriod>(fundingPeriods));

            //Act
            var result = _course.FundingCapOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectCap, result);
        }

        [TestCase("2018-07-31", 0, Description = "Before first funding band")]
        [TestCase("2018-08-01", 1, Description = "Before first funding band but withing same month as first funding band")]
        [TestCase("2018-08-15", 1, Description = "Within (only-day of) first funding band")]
        [TestCase("2018-08-16", 2, Description = "Within (only-day of) second funding band")]
        [TestCase("2018-08-17", 3, Description = "At start of third funding band")]
        [TestCase("2018-08-26", 0, Description = "Beyond end of last funding band")]
        public void AndMultipleBandsInMonthThenTheApplicableFundingPeriodIsUsed(DateTime effectiveDate, int expectCap)
        {
            var courseAndFundingBandStart = new DateTime(2018, 08, 15);
            var courseAndFundingBandEnd = new DateTime(2018, 08, 25);

            
            var fundingPeriods = new List<StandardFundingPeriod>
            {
                new StandardFundingPeriod
                {
                    EffectiveFrom = courseAndFundingBandStart,
                    EffectiveTo = new DateTime(2018,08,15),
                    FundingCap = 1
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,16),
                    EffectiveTo = new DateTime(2018,08,16),
                    FundingCap = 2
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,17),
                    EffectiveTo = courseAndFundingBandEnd,
                    FundingCap = 3
                }
            };
            _course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1", "Test", ProgrammeType.Standard,
                courseAndFundingBandStart, courseAndFundingBandEnd, new List<IFundingPeriod>(fundingPeriods));

            //Act
            var result = _course.FundingCapOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectCap, result);
        }

        [TestCase("2018-08-01", 1, Description = "Within first open-start funding band")]
        [TestCase("2018-08-15", 1, Description = "At end of first open-start funding band")]
        [TestCase("2018-08-16", 2, Description = "At start of second funding band (in same month as first)")]
        [TestCase("2018-08-21", 3, Description = "Start of third funding band")]
        [TestCase("2020-01-01", 3, Description = "Within third open-ended funding band")]
        public void AndFirstFundingPeriodHasNullEffectiveFromAndMultipleBandsInMonthThenTheApplicableFundingPeriodIsUsed(DateTime effectiveDate, int expectCap)
        {
            var courseAndFundingBandStart = (DateTime?)null;
            var courseAndFundingBandEnd = (DateTime?)null;

            var fundingPeriods = new List<StandardFundingPeriod>
            {
                new StandardFundingPeriod
                {
                    EffectiveFrom = courseAndFundingBandStart,
                    EffectiveTo = new DateTime(2018,08,15),
                    FundingCap = 1
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,16),
                    EffectiveTo = new DateTime(2018,08,20),
                    FundingCap = 2
                },
                new StandardFundingPeriod
                {
                    EffectiveFrom = new DateTime(2018,08,21),
                    EffectiveTo = courseAndFundingBandEnd,
                    FundingCap = 3
                }
            };
            _course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1", "Test", ProgrammeType.Standard,
                courseAndFundingBandStart, courseAndFundingBandEnd, new List<IFundingPeriod>(fundingPeriods));

            //Act
            var result = _course.FundingCapOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectCap, result);
        }

        [Test]
        public void IfThereAreNoFundingPeriodsThenCapShouldBeZero()
        {
            //Arrange
            _course.FundingPeriods = new List<TrainingProgrammeFundingPeriod>();

            //Act
            var result = _course.FundingCapOn(new DateTime(2018, 05, 15));

            //Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FundingPeriodsAreEffectiveUntilTheEndOfTheDay()
        {
            //Act
            var result = _course.FundingCapOn(new DateTime(2018, 7, 31, 23, 59, 59));

            //Assert
            Assert.AreEqual(5000, result);
        }
    }
}