using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmploymentPriceValidationTests
    {

        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task For_PortableApprenticeships_EmploymentPrice_Should_Not_Be_(int? value)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.EndDate.Value.AddMonths(-1);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = value;

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("You must add the agreed price for this employment", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EmploymentPrice", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentPrice_Should_Not_Be_More_Than_Course_Price()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.EndDate.Value.AddMonths(-1);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = fixture.Apprenticeship.Cost + 1;

            var result = await fixture.Validate(request);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("This price must not be more than than the total agreed apprenticeship price", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EmploymentPrice", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentPrice_Should_Not_Be_More_Than_100K()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.EndDate.Value.AddMonths(-1);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = Constants.MaximumApprenticeshipCost + 1;

            var result = await fixture.Validate(request);

            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("The agreed price for this employment must be £100,000 or less", result.Errors[0].ErrorMessage);
            Assert.AreEqual("EmploymentPrice", result.Errors[0].PropertyName);
        }
    }
}
