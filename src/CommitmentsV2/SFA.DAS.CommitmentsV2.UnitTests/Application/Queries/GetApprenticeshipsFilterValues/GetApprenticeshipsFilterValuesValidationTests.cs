using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipsFilterValues
{
    [TestFixture]
    public class GetApprenticeshipsFilterValuesValidationTests
    {
        [TestCase(0, 1, true)]
        [TestCase(1, 0, true)]
        [TestCase(null, 1, true)]
        [TestCase(1, null, true)]
        [TestCase(1, 1, false)]
        [TestCase(0, 0, false)]
        [TestCase(null, null, false)]
        [TestCase(null, 0, false)]
        [TestCase(0, null, false)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(long? providerId, long? employerAccountId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsFilterValuesQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsFilterValuesQuery
            {
                ProviderId = providerId,
                EmployerAccountId = employerAccountId
            });

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
