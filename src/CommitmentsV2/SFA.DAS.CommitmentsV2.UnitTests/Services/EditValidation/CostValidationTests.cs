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

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Enter the total agreed training cost"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Cost"));
            });
        }

        [Test]
        public async Task Cost_Should_Be_More_Than_zero()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(cost: 0);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Enter the total agreed training cost"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Cost"));
            });
        }

        [TestCase(0.10, false)]
        [TestCase(11.11, false)]
        [TestCase(99999, true)]
        [TestCase(1, true)]
        [TestCase(7500.10, false)]
        [TestCase(7500.00, true)]
        [TestCase(2000.0, true)]
        public async Task Cost_ShouldNot_Be_In_Fraction(decimal testCost, bool isValid)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(cost: testCost);

            var result = await fixture.Validate(request);

            if (isValid)
            {   
                Assert.That(result.Errors, Is.Empty);
            }
            else
            {
                Assert.That(result.Errors, Has.Count.EqualTo(1));
                Assert.Multiple(() =>
                {
                    Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Enter the total agreed training cost"));
                    Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Cost"));
                });
            }
        }

        [Test]
        public async Task Cost_Should_Not_Be_More_Than_100000()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(cost: 100001);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The total cost must be £100,000 or less"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Cost"));
            });
        }
    }
}
