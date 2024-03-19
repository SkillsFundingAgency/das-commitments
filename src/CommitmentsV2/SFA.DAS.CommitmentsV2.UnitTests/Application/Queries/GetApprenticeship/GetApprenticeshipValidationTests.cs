using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeship
{
    [TestFixture]
    public class GetApprenticeshipValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedCohortId_ShouldSetIsValidCorrectly(int apprenticeshipId, bool expectedIsValid)
        {
            var validator = new GetApprenticeshipQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipQuery(apprenticeshipId));
            Assert.That(validationResults.IsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
