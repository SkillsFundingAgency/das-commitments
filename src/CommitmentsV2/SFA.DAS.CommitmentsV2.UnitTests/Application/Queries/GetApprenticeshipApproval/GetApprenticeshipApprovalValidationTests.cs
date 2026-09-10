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
        var validationResults = validator.Validate(new GetApprenticeshipApprovalQuery(apprenticeshipId, Guid.NewGuid()));
        validationResults.IsValid.Should().Be(expectedIsValid);
    }

    [Test]
    public void Validate_WithNonEmptyApprovalRequestId_Then_Should_Pass()
    {
        var validator = new GetApprenticeshipApprovalQueryValidator();
        var validationResults = validator.Validate(new GetApprenticeshipApprovalQuery(111, Guid.NewGuid()));
        validationResults.IsValid.Should().Be(true);
    }

    [Test]
    public void Validate_WithEmptyApprovalRequestId_Then_Should_Fail()
    {
        var validator = new GetApprenticeshipApprovalQueryValidator();
        var validationResults = validator.Validate(new GetApprenticeshipApprovalQuery(111, Guid.Empty));
        validationResults.IsValid.Should().Be(false);
    }

}