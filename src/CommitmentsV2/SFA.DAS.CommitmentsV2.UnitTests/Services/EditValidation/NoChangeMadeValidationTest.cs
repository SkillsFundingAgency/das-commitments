using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class NoChangeMadeValidationTest
    {
        [Test]
        public async Task When_No_Change_IsMade()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest();

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
           Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "No change made");
            Assert.AreEqual(result.Errors[0].PropertyName, "NoChangesRequested");
        }
    }
}
