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
        public void When_QueryDoesNotContainCourse_Then_ValidateQueryContainsStandardUId(string standardUId, bool expectedResult)
        {
            var validator = new GetTrainingProgrammeVersionQueryValidator();

            var result = validator.Validate(new GetTrainingProgrammeVersionQuery(standardUId));

            Assert.That(result.IsValid, Is.EqualTo(expectedResult));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("123", true)]
        public void When_QueryDoesNotContainStandardUId_Then_ValidateQueryContainsCourseCode(string courseCode, bool expectedResult)
        {
            var version = "1.0";
            
            var validator = new GetTrainingProgrammeVersionQueryValidator();

            var result = validator.Validate(new GetTrainingProgrammeVersionQuery(courseCode, version));

            Assert.That(result.IsValid, Is.EqualTo(expectedResult));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("1.0", true)]
        public void When_QueryDoesNotContainStandardUId_Then_ValidateQueryContainsVersion(string version, bool expectedResult)
        {
            var courseCode = "123";

            var validator = new GetTrainingProgrammeVersionQueryValidator();

            var result = validator.Validate(new GetTrainingProgrammeVersionQuery(courseCode, version));

            Assert.That(result.IsValid, Is.EqualTo(expectedResult));
        }
    }
}
