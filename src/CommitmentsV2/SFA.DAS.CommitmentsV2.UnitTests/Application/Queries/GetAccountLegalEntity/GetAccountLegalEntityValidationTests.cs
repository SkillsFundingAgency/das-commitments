using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountLegalEntity
{
    [TestFixture]
    public class GetAccountLegalEntityValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetAccountLegalEntityValidator();
            var validationResults = validator.Validate(new GetAccountLegalEntityRequest {AccountLegalEntityId = id});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}