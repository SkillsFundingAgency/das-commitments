using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDataLocks
{
    [TestFixture]
    public class GetDataLocksQueryValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedApprenticeshipId(int apprenticeshipId, bool expectedIsValid)
        {
            var validator = new GetDataLocksQueryValidator();
            var validationResults = validator.Validate(new GetDataLocksQuery(apprenticeshipId));
            Assert.That(validationResults.IsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
