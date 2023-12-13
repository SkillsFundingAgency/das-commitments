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

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("You must add the agreed price for this employment"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentPrice"));
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentPrice_Should_Not_Be_More_Than_Course_Price()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.EndDate.Value.AddMonths(-1);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = (int)fixture.Apprenticeship.Cost + 1;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("This price must not be more than the total agreed apprenticeship price"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentPrice"));
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

            Assert.That(result.Errors.Count, Is.EqualTo(2));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The agreed price for this employment must be £100,000 or less"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentPrice"));
        }
    }
}
