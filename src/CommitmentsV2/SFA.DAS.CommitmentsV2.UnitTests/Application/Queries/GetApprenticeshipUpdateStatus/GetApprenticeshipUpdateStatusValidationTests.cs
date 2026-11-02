using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdateStatus;

[TestFixture]
public class GetApprenticeshipUpdateStatusValidationTests
{
    [TestCase(-1, false)]
    [TestCase(0, false)]
    [TestCase(1, true)]
    public void Validate_WithSpecifiedApprenticeshipId(long apprenticeshipId, bool expectedIsValid)
    {
        var validator = new GetApprenticeshipUpdateStatusQueryValidator();
        var validationResults = validator.Validate(new GetApprenticeshipUpdateStatusQuery(apprenticeshipId));
        validationResults.IsValid.Should().Be(expectedIsValid);
    }
}
