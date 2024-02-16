using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmployerRefValidationTests
    {
        [Test]
        public async Task EmployerReference_Should_Not_Be_More_Than_20_Characters()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupAuthenticationContextAsEmployer();
            var request = fixture.CreateValidationRequest(employerRef: "123456789012345678901");

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The Reference must be 20 characters or fewer"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmployerReference"));
            });
        }
    }
}
