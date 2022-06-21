using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetSupportCohortSummary
{
    [TestFixture]
    public class GetSupportCohortSummaryValidationTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetCohortSummaryQueryValidator();
            var validationResults = validator.Validate(new GetCohortSummaryQuery(id));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
