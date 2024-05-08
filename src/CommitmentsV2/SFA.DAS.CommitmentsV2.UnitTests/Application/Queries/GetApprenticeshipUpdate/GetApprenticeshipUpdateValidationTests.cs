using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdate
{
    [TestFixture]
    public class GetApprenticeshipUpdateValidationTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithSpecifiedAppprenticeshipId(int apprenticeshipId, bool expectedIsValid)
        {
            var validator = new GetApprenticeshipUpdateQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipUpdateQuery(apprenticeshipId, null));
            Assert.That(validationResults.IsValid, Is.EqualTo(expectedIsValid));
        }

        [TestCase(null, true)]
        [TestCase(ApprenticeshipUpdateStatus.Approved, true)]
        [TestCase(ApprenticeshipUpdateStatus.Deleted, true)]
        [TestCase(ApprenticeshipUpdateStatus.Superceded, true)]
        public void DontValidate_ApprenticeshipUpdateStatus(ApprenticeshipUpdateStatus? status, bool expectedIsValid)
        {
            var validator = new GetApprenticeshipUpdateQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipUpdateQuery(1, status));
            Assert.That(validationResults.IsValid, Is.EqualTo(expectedIsValid));
        }
    }
}
