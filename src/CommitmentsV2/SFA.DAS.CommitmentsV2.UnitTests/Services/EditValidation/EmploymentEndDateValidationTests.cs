using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmploymentEndDateValidationTests
    {
        [Test]
        public async Task For_PortableApprenticeships_EmploymentEndDate_Should_Not_Be_Null()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentPrice: 100);
            request.EmploymentEndDate = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("You must add the employment end date"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentEndDate"));
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentEndDate_Should_Not_Be_after_Than_Course_EndDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.EndDate.Value.AddMonths(1);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = (int)fixture.Apprenticeship.Cost - 1;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("This date must not be later than the projected apprenticeship training end date"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentEndDate"));
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentEndDate_Should_Not_Be_Within_3_Months_Of_Course_StartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employEndDate = fixture.Apprenticeship.StartDate.Value.AddMonths(2);
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentEndMonth: employEndDate.Month, employmentEndYear: employEndDate.Year);
            request.EmploymentPrice = (int)fixture.Apprenticeship.Cost - 1;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("This date must be at least 3 months later than the planned apprenticeship training start date"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentEndDate"));
        }

        [Test]
        public async Task For_PortableApprenticeships_EmploymentDateTime_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var employPrice = (int)fixture.Apprenticeship.Cost.Value - 100;
            var request = fixture.CreateValidationRequest(deliveryModel: DeliveryModel.PortableFlexiJob,
                employmentPrice: employPrice);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("You must add the employment end date"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EmploymentEndDate"));
        }
    }
}
