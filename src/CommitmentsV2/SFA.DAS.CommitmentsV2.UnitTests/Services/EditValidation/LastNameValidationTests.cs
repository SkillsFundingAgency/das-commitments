using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class LastNameValidationTests
    {
        [Test]
        public async Task LastName_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.LastName = string.Empty;

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Last name must be entered"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("LastName"));
        }

        [TestCase("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567891", false)]
        [TestCase("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", true)]
        public async Task LastName_Length_Should_Be_No_Longer_Than_100_Characters(string LastName, bool isValid)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(lastName: LastName);

            var result = await fixture.Validate(request);

            if (isValid)
            {
                Assert.That(0, Is.EqualTo(result.Errors.Count));
            }
            else
            {
                Assert.That(result.Errors.Count, Is.EqualTo(1));
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("You must enter a last name that's no longer than 100 characters"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("LastName"));
            }
        }
    }
}
