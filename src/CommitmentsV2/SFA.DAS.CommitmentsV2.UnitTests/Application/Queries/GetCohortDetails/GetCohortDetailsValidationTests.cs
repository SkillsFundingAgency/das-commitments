using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortDetails;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortDetails
{
    [TestFixture]
    public class GetCohortDetailsValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetCohortDetailsQueryValidator();
            var validationResults = validator.Validate(new GetCohortDetailsQuery(id));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
