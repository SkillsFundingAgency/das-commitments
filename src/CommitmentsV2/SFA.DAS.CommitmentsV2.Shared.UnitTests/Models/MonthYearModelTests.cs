using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Models
{
    [TestFixture]
    public class MonthYearModelTests
    {
        [Test]
        public void Constructor_WithYearMonth_ShouldNotThrowException()
        {
            var dt = new MonthYearModel("022019");

            Assert.Pass("Completed without exception");
        }

        [Test]
        public void IsValid_ConstructedWithValidYearMonth_ShouldBeTrue()
        {
            var dt = new MonthYearModel("022019");

            Assert.That(dt.IsValid, Is.True);
        }

        [TestCase("000000")]
        [TestCase("132019")]
        [TestCase("120000")]
        public void Constructor_WithInvalidYearMonthElementValues_ShouldThrowArgumentException(string invalidMonthYear)
        {
            Assert.Throws<ArgumentException>(() => new MonthYearModel(invalidMonthYear));
        }

        [Test]
        public void Constructor_WithValidYearMonthElementValues_ShouldMakeDateAvailable()
        {
            var date = new MonthYearModel("122019");

            Assert.That(date.Date, Is.EqualTo(new DateTime(2019, 12, 1)));
        }

        [Test]
        public void DateTime_WithMonthPartChangedAfterConstruction_ShouldMakeNewDateAvailable()
        {
            var date = new MonthYearModel("122019");

            date.Month = 10;
            Assert.That(date.Date, Is.EqualTo(new DateTime(2019, 10, 1)));
        }

        [Test]
        public void DateTime_WithMonthAndYearPartChangedAfterConstruction_ShouldMakeNewDateAvailable()
        {
            var date = new MonthYearModel("122019");

            date.Month = 10;
            date.Year = 2020;

            Assert.That(date.Date, Is.EqualTo(new DateTime(2020, 10, 1)));
        }

        [TestCase("apples")]
        [TestCase("1211111")]
        public void Constructor_WithUnrecogniseableYearMonth_ShouldThrowInvalidArgumentException(string invalidMonthYear)
        {
            Assert.Throws<ArgumentException>(() => new MonthYearModel(invalidMonthYear));
        }

        [TestCase("012019")]
        [TestCase("12019")]
        public void Constructor_WithValidYearMonth_ShouldNotThrowException(string validMonthYear)
        {
            var dt = new MonthYearModel(validMonthYear);

            Assert.Pass("Completed without exception");
        }

        [Test]
        public void Day_WhenSet_ShouldThrowException()
        {
            var dt = new MonthYearModel("022019");

            Assert.Throws<InvalidOperationException>(() => dt.Day = 1);
        }
    }
}
