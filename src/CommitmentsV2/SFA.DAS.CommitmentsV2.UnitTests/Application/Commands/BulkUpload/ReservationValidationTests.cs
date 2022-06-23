using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class ReservationValidationTests
    {
        [Test]
        public async Task Reservation_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.Command.ReservationValidationResults = new Api.Types.Requests.BulkReservationValidationResults();
            fixture.Command.ReservationValidationResults.ValidationErrors.Add(new Api.Types.Requests.BulkReservationValidation { Reason = "The employer has reached their reservations limit. Contact the employer.", RowNumber = 1 });
            
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ReservationId", "The employer has reached their reservations limit. Contact the employer.");
        }

        [Test]
        public async Task Reservation_Validation_Error_Gets_Added_To_Correct_Row()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetProviderRef("012345678901234567890");
            fixture.Command.ReservationValidationResults = new Api.Types.Requests.BulkReservationValidationResults();
            fixture.Command.ReservationValidationResults.ValidationErrors.Add(new Api.Types.Requests.BulkReservationValidation { Reason = "The employer has reached their reservations limit. Contact the employer.", RowNumber = 1 });

            var errors = await fixture.Handle();
            Assert.AreEqual(1, errors.BulkUploadValidationErrors.Count);
            Assert.AreEqual(2, errors.BulkUploadValidationErrors.First().Errors.Count);
            Assert.IsTrue(errors.BulkUploadValidationErrors.All(x => x.RowNumber == 1));
        }
    }
}
