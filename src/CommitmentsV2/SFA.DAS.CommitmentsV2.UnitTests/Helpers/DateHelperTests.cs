using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Helper;

namespace SFA.DAS.CommitmentsV2.UnitTests.Helpers
{
    [TestFixture]
    [Parallelizable]
    public class DateHelperTests
    {
        [TestCase(1, 2, 2003, false)]
        [TestCase(null, 2, 2003, false)]
        [TestCase(1, null, 2003, false)]
        [TestCase(1, 2, null, false)]
        [TestCase(null, null, null, true)]
        public void DateIsEmpty_CheckIfDateIsEmpty(int? day, int? month, int? year, bool expected)
        {
            Assert.AreEqual(DateHelper.DateIsEmpty(day, month, year), expected);
        }

        [TestCase(2, 2003, false)]
        [TestCase(null, 2003, false)]
        [TestCase(1, null, false)]
        [TestCase(null, null, true)]
        public void DateIsEmpty_CheckIfDateIsEmpty(int? month, int? year, bool expected)
        {
            Assert.AreEqual(DateHelper.DateIsEmpty(month, year), expected);
        }

        [TestCase(1, 2, 2003, true)]
        [TestCase(null, 2, 2003, false)]
        [TestCase(1, null, 2003, false)]
        [TestCase(1, 2, null, false)]
        [TestCase(null, null, null, false)]
        public void DateIsValid_CheckIfDateIsValid(int? day, int? month, int? year, bool expected)
        {
            Assert.AreEqual(DateHelper.DateIsValid(day, month, year), expected);
        }

        [TestCase(2, 2003, true)]
        [TestCase(null, 2003, false)]
        [TestCase(1, null, false)]
        [TestCase(null, null, false)]
        public void DateIsValid_CheckIfDateIsValid(int? month, int? year, bool expected)
        {
            Assert.AreEqual(DateHelper.DateIsValid(month, year), expected);
        }

        [TestCase(1, 2, 2003, "2003-02-01")]
        [TestCase(31, 2, 2003, null)]
        [TestCase(null, 2, 2003, null)]
        [TestCase(1, null, 2003, null)]
        [TestCase(1, 2, null, null)]
        [TestCase(null, null, null, null)]
        public void ConvertToNullableDate_ConvertValidDateAndLeaveInvalidAsNull(int? day, int? month, int? year, string expected)
        {
            DateTime? expectedDate = null;
            if (expected != null)
            {
                expectedDate = DateTime.Parse(expected);
            } 

            Assert.AreEqual(DateHelper.ConvertToNullableDate(day, month, year), expectedDate);
        }

        [TestCase(2, 2003, "2003-02-01")]
        [TestCase(14, 2003, null)]
        [TestCase(null, 2003, null)]
        [TestCase(1, null, null)]
        [TestCase(null, null, null)]
        public void ConvertToNullableDate_ConvertValidDateAndLeaveInvalidAsNull(int? month, int? year, string expected)
        {
            DateTime? expectedDate = null;
            if (expected != null)
            {
                expectedDate = DateTime.Parse(expected);
            }

            Assert.AreEqual(DateHelper.ConvertToNullableDate(month, year), expectedDate);
        }

    }
}
