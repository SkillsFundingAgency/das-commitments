using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmployerRefValidationTests
    {
        [Test]
        public async Task EmployerReference_Should_Not_Be_More_Than_20_Characters()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(employerRef: "123456789012345678901");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The Reference must be 20 characters or fewer");
            Assert.AreEqual(result.Errors[0].PropertyName, "EmployerReference");
        }
    }
}
