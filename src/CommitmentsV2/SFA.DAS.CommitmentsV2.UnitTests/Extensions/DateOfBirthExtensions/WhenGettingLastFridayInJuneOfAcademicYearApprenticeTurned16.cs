using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using System;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.DateOfBirthExtensions
{
    [TestFixture]
    public class WhenGettingLastFridayInJuneOfSchoolYearApprenticeTurned16
    {
        [TestCase(2006, 8, 31, 2022, 6, 24)]
        [TestCase(2005, 12, 31, 2022, 6, 24)]
        [TestCase(2006, 9, 1, 2023, 6, 30)]
        [TestCase(2007, 1, 1, 2023, 6, 30)]
        public void ThenShouldReturnCorrectDate(int dobYear, int dobMonth, int dobDay, int expectedYear, int expectedMonth, int expectedDay)
        {
            var result = new DateTime(dobYear, dobMonth, dobDay).GetLastFridayInJuneOfSchoolYearApprenticeTurned16();
            Assert.That(result.Year, Is.EqualTo(expectedYear));
            Assert.That(result.Month, Is.EqualTo(expectedMonth));
            Assert.That(result.Day, Is.EqualTo(expectedDay));
        }
    }
}
