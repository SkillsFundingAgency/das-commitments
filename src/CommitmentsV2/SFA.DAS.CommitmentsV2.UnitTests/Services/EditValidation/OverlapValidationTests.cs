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
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupOverlapService(true, false);
            var request = fixture.CreateValidationRequest(employerRef: "abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help", result.Errors[0].ErrorMessage);
            Assert.AreEqual("StartDate", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task When_EndDate_Overlaps()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupOverlapService(false, true);
            var request = fixture.CreateValidationRequest(employerRef: "abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EndDate", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task When_StarDate_And_EndDate_Overlaps()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupOverlapService(true, true);
            var request = fixture.CreateValidationRequest(employerRef: "abc123");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(2, result.Errors.Count);
            var endDateError = result.Errors.First(x => x.PropertyName == "EndDate");
            Assert.AreEqual("The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help", endDateError.ErrorMessage);
            Assert.AreEqual("EndDate", endDateError.PropertyName);

            var startDateError = result.Errors.First(x => x.PropertyName == "StartDate");
            Assert.AreEqual("The date overlaps with existing training dates for the same apprentice. Please check the date - contact your training provider for help", startDateError.ErrorMessage);
            Assert.AreEqual("StartDate", startDateError.PropertyName);
        }
    }
}
