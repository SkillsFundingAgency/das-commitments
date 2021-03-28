using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EndDateValidationTests
    {
        [Test]
        public async Task EndDate_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.EndDate = null;

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The end date is not valid", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EndDate", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_DasStartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(endYear: 2017, endMonth: 4);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The end date must not be earlier than May 2017", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EndDate", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_Start()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(endYear: fixture.StartDate.Value.Year, endMonth: fixture.StartDate.Value.Month -1);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The end date must not be on or before the start date", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EndDate", result.Errors[0].PropertyName);
        }
    }
}
