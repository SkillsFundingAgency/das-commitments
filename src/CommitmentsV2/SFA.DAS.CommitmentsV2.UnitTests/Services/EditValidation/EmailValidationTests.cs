using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmailValidationTests
    {
        [Test]
        public async Task When_Email_Does_Not_Exist_On_Apprenticeship_Then_No_Change_Can_Be_Requested()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(email: "a@a.com");

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Email update cannot be requested");
            Assert.AreEqual("Email", result.Errors[0].PropertyName);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_Email_Cannot_Be_Null_Or_Empty(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Email address cannot be blank");
            Assert.AreEqual(result.Errors[1].ErrorMessage, "Please enter a valid email address");
        }

        [TestCase("@")]
        [TestCase("paul@@a.com")]
        [TestCase("p@aul@a.com")]
        [TestCase("asa@a")]
        [TestCase("\\asa@a.com")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_New_Email_Cannot_Be_Invalid(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Please enter a valid email address");
            Assert.AreEqual("Email", result.Errors[0].PropertyName);
        }

        [TestCase("paul@a.com")]
        [TestCase("asa@a.ie.uk")]
        [TestCase("asa@a.tv")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_New_Email_Must_Be_Valid(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
