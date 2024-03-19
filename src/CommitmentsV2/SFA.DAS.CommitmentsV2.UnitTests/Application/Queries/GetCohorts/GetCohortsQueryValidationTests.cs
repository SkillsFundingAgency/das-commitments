using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohorts
{
    [TestFixture]
    public class GetCohortsQueryValidationTests
    {
        [TestCase(1, null, true)]
        [TestCase(null, null, false)]
        [TestCase(null, 1, true)]
        [TestCase(1, 1, true)]
        public void Validate_WithAccountIdAndProviderId_ShouldSetIsValidCorrectly(long? accountId, long? providerId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetCohortsQueryValidator();
            var validationResults = validator.Validate(new GetCohortsQuery(accountId, providerId));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
