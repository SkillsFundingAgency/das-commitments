using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdate
{
    [TestFixture]
    public class GetApprenticeshipUpdateValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedCohortId_ShouldSetIsValidCorrectly(int apprenticeshipId, bool expectedIsValid)
        {
            var validator = new GetApprenticeshipUpdateQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipUpdateQuery(apprenticeshipId, null));
            Assert.AreEqual(expectedIsValid, validationResults.IsValid);
        }
    }
}
