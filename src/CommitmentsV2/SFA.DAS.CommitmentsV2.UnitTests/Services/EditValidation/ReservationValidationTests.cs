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
            fixture.SetupMockContextApprenticeship().SetupReservationValidationService();
            var request = fixture.CreateValidationRequest(employerRef: "abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Reason", result.Errors[0].ErrorMessage);
            Assert.AreEqual("CourseCode", result.Errors[0].PropertyName);
        }
    }
}
