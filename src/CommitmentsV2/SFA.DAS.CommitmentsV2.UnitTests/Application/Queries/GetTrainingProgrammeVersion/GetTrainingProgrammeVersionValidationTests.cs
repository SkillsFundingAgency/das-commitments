using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetTrainingProgrammeVersion
{
    [TestFixture]
    public class GetTrainingProgrammeVersionValidationTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("ST0001_1.0", true)]
        public void ValidateQueryContains_StandardUId(string standardUId, bool expectedResult)
        {
            var validator = new GetTrainingProgrammeVersionQueryValidator();

            var result = validator.Validate(new GetTrainingProgrammeVersionQuery(standardUId));

            Assert.AreEqual(expectedResult, result.IsValid);
        }
    }
}
