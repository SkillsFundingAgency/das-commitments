using NUnit.Framework;
using System;
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

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Delivery model is required", result.Errors[0].ErrorMessage);
            Assert.AreEqual("DeliveryModel", result.Errors[0].PropertyName);
        }
    }
}
