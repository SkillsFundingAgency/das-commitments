using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortEmailOverlaps
{
    [TestFixture]
    public class GetCohortEmailOverlapsValidationTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetCohortEmailOverlapsQueryValidator();
            var validationResults = validator.Validate(new GetCohortEmailOverlapsQuery(id));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
