using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipLockedFields;

[TestFixture]
public class GetApprenticeshipLockedFieldsQueryValidatorTests
{
    [TestCase(-1, false)]
    [TestCase(0, false)]
    [TestCase(1, true)]
    public void Validate_WithSpecifiedApprenticeshipId(long apprenticeshipId, bool expectedIsValid)
    {
        var validator = new GetApprenticeshipLockedFieldsQueryValidator();
        var validationResults = validator.Validate(new GetApprenticeshipLockedFieldsQuery(apprenticeshipId));
        validationResults.IsValid.Should().Be(expectedIsValid);
    }
}
