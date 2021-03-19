using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class CostValidationTests
    {
        [Test]
        public async Task Cost_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(cost: null);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Enter the total agreed training cost");
            Assert.AreEqual(result.Errors[0].PropertyName, "Cost");
        }

        [Test]
        public async Task Cost_Should_Be_More_Than_zero()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(cost: 0);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Enter the total agreed training cost");
            Assert.AreEqual(result.Errors[0].PropertyName, "Cost");
        }

        [Test]
        public async Task Cost_Should_Not_Be_More_Than_100000()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(cost: 100001);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The total cost must be £100,000 or less");
            Assert.AreEqual(result.Errors[0].PropertyName, "Cost");
        }
    }
}
