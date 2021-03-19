using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class DateOfBirthValidationTests
    {
        [Test]
        public async Task DateOfBirth_Should_Be_Greater_Than_Min_DateOfBirth()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(dobYear: 1899, dobMonth : 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The Date of birth is not valid");
            Assert.AreEqual(result.Errors[0].PropertyName, "DateOfBirth");
        }

        [Test]
        public async Task DateOfBirth_Should_Be_At_least_15_At_start_of_training()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            // The default course start date for these test is 1st Jan 2020 
            var request = fixture.CreateValidationRequest(dobYear: 2006, dobMonth: 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The apprentice must be at least 15 years old at the start of their training");
            Assert.AreEqual(result.Errors[0].PropertyName, "DateOfBirth");
        }

        [Test]
        public async Task DateOfBirth_Must_Be_Younger_Than_115_At_start_of_training()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            // The default course start date for these test is 1st Jan 2020 
            var request = fixture.CreateValidationRequest(dobYear: 1904, dobMonth: 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The apprentice must be younger than 115 years old at the start of their training");
            Assert.AreEqual(result.Errors[0].PropertyName, "DateOfBirth");
        }

        [Test]
        public async Task DateOfBirth_Is_Required()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            // The default course start date for these test is 1st Jan 2020 
            var request = fixture.CreateValidationRequest(dobYear: null);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The Date of birth is not valid");
            Assert.AreEqual(result.Errors[0].PropertyName, "DateOfBirth");
        }

        //TODO: Remove this test.
        //[Test]
        //public async Task DateOfBirth__Should_Be_Valid()
        //{
        //    var fixture = new EditApprenitceshipValidationServiceTestsFixture();
        //    fixture.CreateMockContextApprenitceship();
        //    // The default course start date for these test is 1st Jan 2020 
        //    var request = fixture.CreateValidationRequest(dobMonth : 2, dobDay: 30);

        //    var result = await fixture.Validate(request);

        //    Assert.AreEqual(result.Errors.Count, 1);
        //    Assert.AreEqual(result.Errors[0].ErrorMessage, "The Date of birth is not valid");
        //    Assert.AreEqual(result.Errors[0].PropertyName, "DateOfBirth");
        //}
    }
}
