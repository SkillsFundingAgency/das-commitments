using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EndDateValidationTests
    {
        [Test]
        public async Task EndDate_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest();
            request.EndDate = null;

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The end date is not valid");
            Assert.AreEqual(result.Errors[0].PropertyName, "EndDate");
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_DasStartDate()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(endYear: 2017, endMonth: 4);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The end date must not be earlier than May 2017");
             Assert.AreEqual(result.Errors[0].PropertyName, "EndDate");
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_Start()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            // The start date is set as 2020 1
            var request = fixture.CreateValidationRequest(endYear: 2019, endMonth: 12);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The end date must not be on or before the start date");
             Assert.AreEqual(result.Errors[0].PropertyName, "EndDate");
        }
    }
}
