using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountSummary
{
    [TestFixture]
    public class GetAccountSummaryValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetAccountSummaryQueryValidator();
            var validationResults = validator.Validate(new GetAccountSummaryQuery { AccountId = id });

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
