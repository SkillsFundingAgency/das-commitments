using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    public class MonthYearStringExtensionsTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("XXXX", false)]
        [TestCase("132020", false)]
        [TestCase("022020", true)]
        [TestCase("22020", true)]
        public void ThenIsValidMonthYearCheckIsCalled(string dateMonth, bool isValid)
        {
            //Act
            var actual = dateMonth.IsValidMonthYear();

            //Assert
            Assert.That(actual, Is.EqualTo(isValid));
        }
    }
}
