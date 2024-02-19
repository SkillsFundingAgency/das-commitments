using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class DeliveryModelValidationTests
    {
        [Test]
        public async Task DeliveryCode_Is_Mandatory()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.DeliveryModel = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Delivery model is required"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DeliveryModel"));
            });
        }
    }
}
