namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class NoChangeMadeValidationTest
    {
        [Test]
        public async Task When_No_Change_IsMade()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("No change made: you need to amend details or cancel"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("ApprenticeshipId"));
            });
        }
    }
}
