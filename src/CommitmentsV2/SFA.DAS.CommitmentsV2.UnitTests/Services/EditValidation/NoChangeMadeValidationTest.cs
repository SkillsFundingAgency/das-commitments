namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation;

public class NoChangeMadeValidationTest
{
    [Test]
    public async Task When_No_Change_IsMade()
    {
        var fixture = new EditApprenticeshipValidationServiceTestsFixture();
        fixture.SetupMockContextApprenticeship();
        var request = fixture.CreateValidationRequest();

        var result = await fixture.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.ErrorMessage == "No change made: you need to amend details or cancel" &&
            error.PropertyName == "ApprenticeshipId");
    }

    [Test]
    public async Task When_No_Change_IsMade_And_Current_Price_IsSplit()
    {
        var fixture = new EditApprenticeshipValidationServiceTestsFixture();
        fixture.SetupMockContextApprenticeship();
        fixture.Apprenticeship.PriceHistory.First().TrainingPrice = 150;
        fixture.Apprenticeship.PriceHistory.First().AssessmentPrice = 50;
        var request = fixture.CreateValidationRequest();

        var result = await fixture.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.ErrorMessage == "No change made: you need to amend details or cancel" &&
            error.PropertyName == "ApprenticeshipId");
    }

    [Test]
    public async Task When_Training_Price_IsChanged_And_Current_Price_IsSplit()
    {
        var fixture = new EditApprenticeshipValidationServiceTestsFixture();
        fixture.SetupMockContextApprenticeship();
        fixture.Apprenticeship.PriceHistory.First().TrainingPrice = 150;
        fixture.Apprenticeship.PriceHistory.First().AssessmentPrice = 50;
        var request = fixture.CreateValidationRequest();
        request.TrainingPrice = 175;

        var result = await fixture.Validate(request);

        result.Errors.Should().NotContain(error =>
            error.ErrorMessage == "No change made: you need to amend details or cancel" &&
            error.PropertyName == "ApprenticeshipId");
    }
}
