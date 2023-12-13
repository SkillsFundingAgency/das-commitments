using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class ReservationValidationTests
    {
        [Test]
        public async Task Reservation_Validations_Are_Returned()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupAuthenticationContextAsEmployer().SetupReservationValidationService();
            var request = fixture.CreateValidationRequest(employerRef: "abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Reason"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("CourseCode"));
        }
    }
}
