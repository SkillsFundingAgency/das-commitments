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
            Assert.That(value.ToGdsFormat(), Is.EqualTo(expectedResult));
        }

        [TestCase("2019-03-01", "1 March 2019")]
        [TestCase("2020-10-20", "20 October 2020")]
        public void ToGdsFormatLongMonthReturnsFormattedResultCorrectly(DateTime value, string expectedResult)
        {
            Assert.That(value.ToGdsFormatLongMonth(), Is.EqualTo(expectedResult));
        }

        [TestCase("2019-03-01", "Mar 2019")]
        [TestCase("2020-10-20", "Oct 2020")]
        public void ToGdsFormatWithoutDayReturnsFormattedResultCorrectly(DateTime value, string expectedResult)
        {
            Assert.That(value.ToGdsFormatWithoutDay(), Is.EqualTo(expectedResult));
        }

        [TestCase("2019-03-01", "March 2019")]
        [TestCase("2020-10-20", "October 2020")]
        public void ToGdsFormatLongMonthWithoutDayReturnsFormattedResultCorrectly(DateTime value, string expectedResult)
        {
            Assert.That(value.ToGdsFormatLongMonthWithoutDay(), Is.EqualTo(expectedResult));
        }

    }
}
