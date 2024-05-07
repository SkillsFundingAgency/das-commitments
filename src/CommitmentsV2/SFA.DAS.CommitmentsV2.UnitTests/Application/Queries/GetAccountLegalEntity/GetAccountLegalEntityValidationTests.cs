using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountLegalEntity
{
    [TestFixture]
    public class GetAccountLegalEntityValidationTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetAccountLegalEntityQueryValidator();
            var validationResults = validator.Validate(new GetAccountLegalEntityQuery {AccountLegalEntityId = id});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
