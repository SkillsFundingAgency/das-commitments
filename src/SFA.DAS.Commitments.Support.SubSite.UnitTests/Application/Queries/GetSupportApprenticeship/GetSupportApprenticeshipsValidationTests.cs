using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Application.Queries.GetSupportApprenticeship
{
    [TestFixture]
    public class GetSupportApprenticeshipsValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedCohortId_ShouldSetIsValidCorrectly(int cohortId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetDraftApprenticeshipsQueryValidator();
            var validationResults = validator.Validate(new GetDraftApprenticeshipsQuery(cohortId));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.That(actualIsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
