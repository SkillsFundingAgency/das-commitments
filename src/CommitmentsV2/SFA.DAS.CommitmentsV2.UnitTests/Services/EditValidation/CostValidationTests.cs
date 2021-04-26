using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class CostValidationTests
    {
        [Test]
        public async Task Cost_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();

            var request = fixture.CreateValidationRequest();
            request.Cost = null;

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Enter the total agreed training cost", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Cost", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task Cost_Should_Be_More_Than_zero()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(cost: 0);

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Enter the total agreed training cost", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Cost", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task Cost_Should_Not_Be_More_Than_100000()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(cost: 100001);

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("The total cost must be £100,000 or less", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Cost", result.Errors[0].PropertyName);
        }
    }
}
