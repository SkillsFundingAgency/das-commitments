using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class ULNValidationTests
    {
        [Test]
        public async Task Employer_Should_Not_Be_Able_To_Change_ULN()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(uln: "555");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
           Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Employer cannot modify ULN");
            Assert.AreEqual(result.Errors[0].PropertyName, "ULN");
        }
    }
}
