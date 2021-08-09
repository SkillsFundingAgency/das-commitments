using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetStandardOptions
{
    [TestFixture]
    public class GetStandardOptionValidationTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("ST0001_1.0", true)]
        public void ValidateQueryContains_StandardUId(string standardUId, bool expectedResult)
        {
            var validator = new GetStandardOptionsValidator();

            var result = validator.Validate(new GetStandardOptionsQuery(standardUId));

            Assert.AreEqual(expectedResult, result.IsValid);
        }
    }
}
