﻿using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class IntegerExtensionsTests
    {
        [TestCase(1, "£1")]
        [TestCase(0, "£0")]
        [TestCase(123456, "£123,456")]
        public void ToGdsCostFormatReturnsFormattedResultCorrectly(int value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, value.ToGdsCostFormat());
        }

    }
}
