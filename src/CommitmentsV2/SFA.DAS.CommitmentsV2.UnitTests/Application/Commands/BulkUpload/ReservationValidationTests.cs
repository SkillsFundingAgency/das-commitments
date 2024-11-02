namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class ReservationValidationTests
    {
        [Test]
        public async Task Reservation_Validation_Error()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.Command.ReservationValidationResults = new Api.Types.Requests.BulkReservationValidationResults();
            fixture.Command.ReservationValidationResults.ValidationErrors.Add(new Api.Types.Requests.BulkReservationValidation { Reason = "The employer has reached their reservations limit. Contact the employer.", RowNumber = 1 });
            
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "ReservationId", "The employer has reached their reservations limit. Contact the employer.");
        }

        [Test]
        public async Task Reservation_Validation_Error_Gets_Added_To_Correct_Row()
        {
            using  var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetProviderRef("012345678901234567890");
            fixture.Command.ReservationValidationResults = new Api.Types.Requests.BulkReservationValidationResults();
            fixture.Command.ReservationValidationResults.ValidationErrors.Add(new Api.Types.Requests.BulkReservationValidation { Reason = "The employer has reached their reservations limit. Contact the employer.", RowNumber = 1 });

            var errors = await fixture.Handle();
            Assert.That(errors.BulkUploadValidationErrors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(errors.BulkUploadValidationErrors.First().Errors, Has.Count.EqualTo(2));
                Assert.That(errors.BulkUploadValidationErrors.All(x => x.RowNumber == 1), Is.True);
            });
        }
    }

    public class PriorLearningValidationTests
    {
        [Test]
        public async Task Prior_learning_is_not_required_when_start_is_before_aug2022()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(null, null, null);

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.Should().BeEmpty();
        }

        [Test]
        public async Task Prior_learning_should_not_be_entered_when_start_is_before_aug2022()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(true, null, null);

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
        }

        [Test]
        public async Task Prior_Learning_Validation_Error()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: null);

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }

        [Test]
        public async Task RecognisePriorLearning_Field_Validation_Error()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = "XXX";

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }
    }
}
