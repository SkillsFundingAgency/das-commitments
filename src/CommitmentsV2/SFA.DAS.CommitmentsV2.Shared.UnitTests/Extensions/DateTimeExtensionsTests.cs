using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [TestCase("2019-03-01", "1 Mar 2019")]
        [TestCase("2020-10-20", "20 Oct 2020")]
        public void ToGdsFormatReturnsFormattedResultCorrectly(DateTime value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, value.ToGdsFormat());
        }

        [TestCase("2019-03-01", "Mar 2019")]
        [TestCase("2020-10-20", "Oct 2020")]
        public void ToGdsFormatWithoutDayReturnsFormattedResultCorrectly(DateTime value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, value.ToGdsFormatWithoutDay());
        }
    }
}
