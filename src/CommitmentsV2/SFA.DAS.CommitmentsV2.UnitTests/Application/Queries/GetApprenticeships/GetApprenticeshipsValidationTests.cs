using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    [TestFixture]
    public class GetApprenticeshipsValidationTests
    {
        [TestCase( (uint) 0, false)]
        [TestCase( (uint) 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(uint id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsRequest {ProviderId = id});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
