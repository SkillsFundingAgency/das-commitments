using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprentices
{
    [TestFixture]
    public class GetApprovedApprenticesValidationTests
    {
        [TestCase( (uint) 0, false)]
        [TestCase( (uint) 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(uint id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprovedApprenticesValidator();
            var validationResults = validator.Validate(new GetApprovedApprenticesRequest {ProviderId = id});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
