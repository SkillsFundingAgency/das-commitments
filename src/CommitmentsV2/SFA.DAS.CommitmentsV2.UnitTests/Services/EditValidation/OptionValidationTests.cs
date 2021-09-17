using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class OptionValidationTests
    {
        [Test]
        public async Task When_RequestContainsAnOption_AndOptionIsAvailable_Then_ReturnValid()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship()
                .SetUpGetTrainingProgrammeVersionByCourseCodeAndVersion(hasOptions: true);
            var request = fixture.CreateValidationRequest(option: "Option 1");

            var result = await fixture.Validate(request);

            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public async Task When_RequestContainsAnOption_And_OptionIsNotAvailable_Then_ReturnInvalidValid()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship()
                .SetUpGetTrainingProgrammeVersionByCourseCodeAndVersion(hasOptions: true);
            var request = fixture.CreateValidationRequest(option: "Not an option");

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The chosen option is not available for this standard version");
            Assert.AreEqual("Option", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task When_RequestContainsAnOption_And_StandardVersionHasNoOptions_Then_ReturnInvalidValid()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship()
                .SetUpGetTrainingProgrammeVersionByCourseCodeAndVersion(hasOptions: false);
            var request = fixture.CreateValidationRequest(option: "Not an option");

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "An option cannot be selected for this standard version");
            Assert.AreEqual("Option", result.Errors[0].PropertyName);
        }
    }
}
