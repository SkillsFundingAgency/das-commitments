using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticeships
{
    [TestFixture]
    public class GetDraftApprenticeshipsValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedCohortId_ShouldSetIsValidCorrectly(int cohortId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetDraftApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetDraftApprenticeshipsRequest(cohortId));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
