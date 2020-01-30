using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprenticesFilterValues
{
    [TestFixture]
    public class GetApprovedApprenticesFilterValuesValidationTests
    {
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(long providerId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsFilterValuesQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsFilterValuesQuery {ProviderId = providerId});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
