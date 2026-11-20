using FluentAssertions;
using SFA.DAS.CommitmentsV2.Shared.Services;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services;

[TestFixture]
public class AgeCalculationServiceTests
{
    private AgeCalculationService _ageCalculationService;

    [SetUp]
    public void Setup()
    {
        _ageCalculationService = new AgeCalculationService();
    }

    [Test]
    public void CalculateLearnerAgeComparedToASpecificDate_ShouldReturnNull_WhenStartDateIsNull()
    {
        var result = _ageCalculationService.CalculateLearnerAgeComparedToASpecificDate(null, new DateTime(2000, 1, 1));

        result.Should().BeNull();
    }

    [Test]
    public void CalculateLearnerAgeComparedToASpecificDate_ShouldReturnNull_WhenDateOfBirthIsNull()
    {
        var result = _ageCalculationService.CalculateLearnerAgeComparedToASpecificDate(new DateTime(2026, 1, 1), null);

        result.Should().BeNull();
    }

    [Test]
    public void CalculateLearnerAgeComparedToASpecificDate_ShouldReturnAge_WhenBirthdayHasOccurred()
    {
        var result = _ageCalculationService.CalculateLearnerAgeComparedToASpecificDate(new DateTime(2029, 6, 15), new DateTime(2000, 1, 1));

        result.Should().Be(29);
    }

    [Test]
    public void CalculateLearnerAgeComparedToASpecificDate_ShouldReturnAge_WhenBirthdayIsToday()
    {
        var result = _ageCalculationService.CalculateLearnerAgeComparedToASpecificDate(new DateTime(2026, 6, 15), new DateTime(2000, 6, 15));

        result.Should().Be(26);
    }

    [Test]
    public void CalculateLearnerAgeComparedToASpecificDate_ShouldReduceAgeByOne_WhenBirthdayHasNotOccurred()
    {
        var result = _ageCalculationService.CalculateLearnerAgeComparedToASpecificDate(new DateTime(2026, 6, 14), new DateTime(2000, 6, 15));

        result.Should().Be(25);
    }

    [Test]
    public void WillLearnerBeAtLeastMinAgeAtStartOfTraining_ShouldReturnFalse_WhenStartDateIsNull()
    {
        var result = _ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(null, new DateTime(2000, 1, 1), 15);

        result.Should().BeFalse();
    }

    [Test]
    public void WillLearnerBeAtLeastMinAgeAtStartOfTraining_ShouldReturnFalse_WhenDateOfBirthIsNull()
    {
        var result = _ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(new DateTime(2026, 1, 1), null, 15);

        result.Should().BeFalse();
    }

    [Test]
    public void WillLearnerBeAtLeastMinAgeAtStartOfTraining_ShouldReturnTrue_WhenAgeEqualsMinAge()
    {
        var result = _ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(new DateTime(2026, 6, 15), new DateTime(1999, 6, 15), 15);

        result.Should().BeTrue();
    }

    [Test]
    public void WillLearnerBeAtLeastMinAgeAtStartOfTraining_ShouldReturnTrue_WhenAgeExceedsMinAge()
    {
        var result = _ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(new DateTime(2026, 6, 15), new DateTime(1990, 1, 1), 15);

        result.Should().BeTrue();
    }

    [Test]
    public void WillLearnerBeAtLeastMinAgeAtStartOfTraining_ShouldReturnFalse_WhenAgeIsBelowMinAge()
    {
        var result = _ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(new DateTime(2026, 6, 14), new DateTime(2016, 6, 15), 15);

        result.Should().BeFalse();
    }

    [Test]
    public void LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining_ShouldReturnFalse_WhenStartDateIsNull()
    {
        var result = _ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(null, new DateTime(2001, 1, 1), 25);

        result.Should().BeFalse();
    }

    [Test]
    public void LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining_ShouldReturnFalse_WhenDateOfBirthIsNull()
    {
        var result = _ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(new DateTime(2026, 1, 1), null, 25);

        result.Should().BeFalse();
    }

    [Test]
    public void LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining_ShouldReturnTrue_WhenAgeIsLessThanMaxAge()
    {
        var result = _ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(new DateTime(2026, 6, 15), new DateTime(2010, 6, 16), 25);

        result.Should().BeTrue();
    }

    [Test]
    public void LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining_ShouldReturnFalse_WhenAgeEqualsMaxAge()
    {
        var result = _ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(new DateTime(2026, 6, 15), new DateTime(2001, 6, 15), 25);

        result.Should().BeFalse();
    }

    [Test]
    public void LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining_ShouldReturnFalse_WhenAgeExceedsMaxAge()
    {
        var result = _ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(new DateTime(2026, 6, 15), new DateTime(1990, 1, 1), 25);

        result.Should().BeFalse();
    }
}