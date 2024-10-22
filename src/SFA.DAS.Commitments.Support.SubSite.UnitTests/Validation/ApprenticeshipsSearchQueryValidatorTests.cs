using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Validation;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Validation;

public class ApprenticeshipsSearchQueryValidatorTests
{
    private ApprenticeshipsSearchQueryValidator _validator;
    private readonly Mock<Learners.Validators.IUlnValidator> _ulnValidator = new();

    [SetUp]
    public void SetUp()
    {
        _validator = new ApprenticeshipsSearchQueryValidator(_ulnValidator.Object);
    }

    [TestCase("5NOT6", false)]
    [TestCase("8NOT7", false)]
    [TestCase("6DIGIT", true)]
    [TestCase("7DIGITS", true)]
    public void WhenSearchByCohort_ShouldValidate(string testCohortId, bool expectedResult)
    {
        // Arrange
        var searchQuery = new ApprenticeshipSearchQuery
        {
            HashedAccountId = "ABC123",
            SearchType = ApprenticeshipSearchType.SearchByCohort,
            SearchTerm = testCohortId
        };
        
        // Act 
        var result = _validator.Validate(searchQuery);

        // Assert
        result.IsValid.Should().Be(expectedResult);
        if (!expectedResult)
        {
            result.Errors.Should().Contain(x => x.ErrorMessage == "Please enter a 6 or 7-digit Cohort number");
        }
    }
}