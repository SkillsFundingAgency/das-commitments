using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Extensions;

namespace SFA.DAS.Commitments.Application.UnitTests.Extensions.DateTimeExtensions
{
    [TestFixture]
    public class WhenGettingFirstOfMonth
    {
        [TestCase("2018-06-15", "2018-06-01 00:00:00")]
        [TestCase("2018-06-01 18:35:14", "2018-06-01 00:00:00")]
        public void ThenTheFirstDayOfTheMonthIsReturned(DateTime value, DateTime expectResult)
        {
            Assert.AreEqual(expectResult, value.FirstOfMonth());
        }
    }
}
