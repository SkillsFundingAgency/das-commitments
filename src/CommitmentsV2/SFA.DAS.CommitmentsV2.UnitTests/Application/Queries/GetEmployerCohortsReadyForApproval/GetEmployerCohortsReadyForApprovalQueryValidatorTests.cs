using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetEmployerCohortsReadyForApproval
{
    [TestFixture]
    public class GetEmployerCohortsReadyForApprovalQueryValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedEmployerAccountId_ShouldSetIsValidCorrectly(int employerAccountId, bool expectedIsValid)
        {
            var validator = new GetEmployerCohortsReadyForApprovalQueryValidator();
            var validationResults = validator.Validate(new GetEmployerCohortsReadyForApprovalQuery(employerAccountId));
            Assert.AreEqual(expectedIsValid, validationResults.IsValid);
        }

    }
}
