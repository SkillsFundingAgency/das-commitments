using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;


namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class DecimalExtensionsTests
    {
        [TestCase(0, "£0")]
        [TestCase(10, "£10")]
        [TestCase(100, "£100")]
        [TestCase(1234, "£1,234")]
        [TestCase(12345, "£12,345")]
        [TestCase(12345.23, "£12,345")]
        public void DecimalValue_FormattedCorrectlyToGdsCostFormat(decimal value, string expectedResult)
        {
            Assert.That(value.ToGdsCostFormat(), Is.EqualTo(expectedResult));
        }
    }
}
