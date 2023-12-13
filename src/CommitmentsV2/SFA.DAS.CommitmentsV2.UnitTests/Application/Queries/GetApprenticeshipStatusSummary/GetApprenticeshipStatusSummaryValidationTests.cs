using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipStatusSummary
{   
    [TestFixture]
    public class GetApprenticeshipStatusSummaryValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedEmployerAccountId_ShouldSetIsValidCorrectly(int employerAccountId, bool expectedIsValid)
        {
            var validator = new GetApprenticeshipStatusSummaryQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipStatusSummaryQuery(employerAccountId));
            Assert.That(validationResults.IsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
