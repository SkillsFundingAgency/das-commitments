namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class FirstNameValidationTests
    {
        [Test]
        public async Task FirstName_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.FirstName = string.Empty;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("First name must be entered"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("FirstName"));
            });
        }

        [TestCase("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567891", false)]
        [TestCase("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", true)]
        public async Task FirstName_Length_Should_Be_No_Longer_Than_100_Characters(string firstName, bool isValid)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(firstName: firstName);

            var result = await fixture.Validate(request);

            if (isValid)
            {
                Assert.That(result.Errors, Is.Empty);
            }
            else
            {
                Assert.That(result.Errors, Has.Count.EqualTo(1));
                Assert.Multiple(() =>
                {
                    Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("You must enter a first name that's no longer than 100 characters"));
                    Assert.That(result.Errors[0].PropertyName, Is.EqualTo("FirstName"));
                });
            }
        }
    }
}
