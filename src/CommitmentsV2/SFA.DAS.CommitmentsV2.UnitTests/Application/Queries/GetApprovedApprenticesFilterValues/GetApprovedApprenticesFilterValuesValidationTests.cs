using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprenticesFilterValues
{
    [TestFixture]
    public class GetApprovedApprenticesFilterValuesValidationTests
    {
        [TestCase( (uint)0, false)]
        [TestCase( (uint)1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(uint providerId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprovedApprenticesFilterValuesQueryValidator();
            var validationResults = validator.Validate(new GetApprovedApprenticesFilterValuesQuery {ProviderId = providerId});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
