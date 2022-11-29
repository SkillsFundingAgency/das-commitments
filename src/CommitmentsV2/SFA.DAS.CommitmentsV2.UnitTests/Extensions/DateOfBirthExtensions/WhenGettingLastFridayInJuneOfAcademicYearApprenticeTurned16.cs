using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using System;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.DateOfBirthExtensions
{
    [TestFixture]
    public class WhenGettingLastFridayInJuneOfAcademicYearApprenticeTurned16
    {
        [TestCase(2006, 1, 1, 2022, 6, 24)]
        [TestCase(2005, 9, 1, 2022, 6, 24)]
        [TestCase(2005, 8, 31, 2021, 6, 25)]
        [TestCase(2001, 1, 1, 2017, 6, 30)]
        public void ThenShouldReturnCorrectDate(int dobYear, int dobMonth, int dobDay, int expectedYear, int expectedMonth, int expectedDay)
        {
            var result = new DateTime(dobYear, dobMonth, dobDay).GetLastFridayInJuneOfAcademicYearApprenticeTurned16();
            Assert.AreEqual(expectedYear, result.Year);
            Assert.AreEqual(expectedMonth, result.Month);
            Assert.AreEqual(expectedDay, result.Day);
        }
    }
}
