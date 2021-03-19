using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class FirstNameValidationTests
    {
        [Test]
        public async Task FirstName_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(firstName: string.Empty);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "First name must be entered");
            Assert.AreEqual(result.Errors[0].PropertyName, "FirstName");
        }

        [TestCase("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567891",false)]
        [TestCase("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", true)]
        public async Task FirstName_Length_Should_Be_No_Longer_Than_100_Characters(string firstName, bool isValid)
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(firstName: firstName);

            var result = await fixture.Validate(request);

            if (isValid)
            {
                Assert.AreEqual(result.Errors.Count, 0);
            }
            else
            {
                Assert.AreEqual(result.Errors.Count, 1);
                Assert.AreEqual(result.Errors[0].ErrorMessage, "You must enter a first name that's no longer than 100 characters");
                Assert.AreEqual(result.Errors[0].PropertyName, "FirstName");
            }
        }
    }
}
