using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProviderCommitmentAgreements
{
    [TestFixture]
    public class GetProviderCommitmentAgreementValidatorTests
    {
        [TestCase(200, true)]
        [TestCase(1, true)]
        [TestCase(0,  false)]
        [TestCase(-1, false)]
        public void Validate_WithProviderId_ShouldSetIsValidCorrectly(long providerId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetProviderCommitmentAgreementValidator();
            var validationResults = validator.Validate(new GetProviderCommitmentAgreementQuery(providerId));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
