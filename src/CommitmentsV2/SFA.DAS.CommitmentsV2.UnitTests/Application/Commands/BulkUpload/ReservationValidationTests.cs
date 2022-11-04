using FluentAssertions;
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

    public class PriorLearningValidationTests
    {
        [Test]
        public async Task Prior_learning_is_not_required_when_start_is_before_aug2022()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(null, null, null);

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.Should().BeEmpty();
        }

        [Test]
        public async Task Prior_learning_should_not_be_entered_when_start_is_before_aug2022()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(true, null, null);

            var errors = await fixture.Handle();

            fixture.ValidateError(errors, "RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
        }

        [Test]
        [Ignore("RPL cannot be mandatory in bulk upload until all Providers' software systems are updated")]
        public async Task Prior_Learning_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: null);

            var errors = await fixture.Handle();

            fixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }

        [Test]
        public async Task RecognisePriorLearning_Field_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = "XXX";

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }

        [Test]
        public async Task Prior_Learning_Duration_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: null, priceReducedBy: 1);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "DurationReducedBy", "Enter the <b>duration</b> this apprenticeship has been reduced by due to prior learning in weeks using numbers only.");
        }

        [Test]
        public async Task Prior_learning_minimum_duration_validation()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: -1);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning must 0 or more.");
        }

        [Test]
        public async Task Prior_learning_maximum_duration_validation()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 1000);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning must be 999 or less.");
        }

        [Test]
        public async Task Prior_Learning_Price_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, priceReducedBy: null);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "PriceReducedBy", "Enter the <b>price</b> this apprenticeship has been reduced by due to prior learning using numbers only.");
        }

        [Test]
        public async Task Prior_learning_minimum_price_validation()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, priceReducedBy: -1);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be 0 or more.");
        }

        [Test]
        public async Task Prior_learning_maximum_price_validation()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, priceReducedBy: 100001);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be £100,000 or less.");
        }

        [Test]
        public async Task Prior_learning_details_should_not_be_present_when_RPL_is_false()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: false, durationReducedBy: 1, priceReducedBy: 1);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning should not be entered when recognise prior learning is false.");
            fixture.ValidateError(errors, "PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning should not be entered when recognise prior learning is false.");
        }
    }
}
