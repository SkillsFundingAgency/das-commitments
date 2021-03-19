using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class ReservationValidationTests
    {
        [Test]
        public async Task Reservation_Validations_Are_Returned()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship().SetupReservationValidationService();
            var request = fixture.CreateValidationRequest(employerRef:"abc");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "Reason");
            Assert.AreEqual(result.Errors[0].PropertyName, "CourseCode");
        }
    }
}
