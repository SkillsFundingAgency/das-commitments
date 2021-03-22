using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class OverlapValidationTests
    {
        [Test]
        public async Task When_StartDate_Overlaps()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship().SetupOverlapService(true, false);
            var request = fixture.CreateValidationRequest(employerRef: "abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
           Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task When_EndDate_Overlaps()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship().SetupOverlapService(false, true);
            var request = fixture.CreateValidationRequest(employerRef:"abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
           Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help");
            Assert.AreEqual(result.Errors[0].PropertyName, "EndDate");
        }

        [Test]
        public async Task When_StarDate_And_EndDate_Overlaps()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship().SetupOverlapService(true, true);
            var request = fixture.CreateValidationRequest(employerRef : "abc123");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 2);
            var endDateError = result.Errors.First(x => x.PropertyName == "EndDate");
            Assert.AreEqual(endDateError.ErrorMessage, "The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help");
            Assert.AreEqual(endDateError.PropertyName, "EndDate");

            var startDateError = result.Errors.First(x => x.PropertyName == "StartDate");
            Assert.AreEqual(startDateError.ErrorMessage, "The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help");
            Assert.AreEqual(startDateError.PropertyName, "StartDate");
        }
    }
}
