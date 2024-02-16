using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class DateOfBirthValidationTests
    {
        [Test]
        public async Task DateOfBirth_Should_Be_Greater_Than_Min_DateOfBirth()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(dobYear: 1899, dobMonth : 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The Date of birth is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Be_At_least_15_At_start_of_training()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 2006, dobMonth: 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be at least 15 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Must_Be_Younger_Than_115_At_start_of_training()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 1904, dobMonth: 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be younger than 115 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Is_Required()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
          
            var request = fixture.CreateValidationRequest();
            request.DateOfBirth = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The Date of birth is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }
    }
}
