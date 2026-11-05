using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipApproval;

[TestFixture]
public class GetApprenticeshipApprovalValidationTests
{
    [TestCase(-1, false)]
    [TestCase(0, false)]
    [TestCase(1, true)]
    public void Validate_WithSpecifiedAppprenticeshipId(int apprenticeshipId, bool expectedIsValid)
    {
        var validator = new GetApprenticeshipApprovalQueryValidator();
        var validationResults = validator.Validate(new GetApprenticeshipApprovalQuery(apprenticeshipId));
        validationResults.IsValid.Should().Be(expectedIsValid);
    }
}
