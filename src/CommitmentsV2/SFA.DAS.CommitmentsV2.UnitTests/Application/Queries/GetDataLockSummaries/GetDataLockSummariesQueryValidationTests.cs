using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDataLockSummaries
{
    [TestFixture]
    public class GetDataLockSummariesQueryValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedApprenticeshipId(int apprenticeshipId, bool expectedIsValid)
        {
            var validator = new GetDataLockSummariesQueryValidator();
            var validationResults = validator.Validate(new GetDataLockSummariesQuery(apprenticeshipId));
            Assert.AreEqual(expectedIsValid, validationResults.IsValid);
        }
    }
}
