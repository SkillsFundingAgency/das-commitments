using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class ULNValidationTests
    {
        [Test]
        public async Task Employer_Should_Not_Be_Able_To_Change_ULN()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(uln: "555");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Employer cannot modify ULN", result.Errors[0].ErrorMessage);
            Assert.AreEqual("ULN", result.Errors[0].PropertyName);
        }
    }
}
