using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningValidationTests
    {
        [Test]
        public async Task Prior_learning_is_not_required_when_start_is_before_aug2022()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(null, null, null);

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.Should().BeEmpty();
        }

        [Test]
        public async Task Prior_learning_should_not_be_entered_when_start_is_before_aug2022()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-07-31");
            fixture.SetPriorLearning(true, null, null);

            var errors = await fixture.Handle();

            fixture.ValidateError(errors, "RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
        }

        [Test]
        public async Task Prior_Learning_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: null);

            var errors = await fixture.Handle();

            fixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }

        [Test]
        public async Task RecognisePriorLearning_Field_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = "XXX";

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }

        [TestCase("TRUE")]
        [TestCase("true")]
        [TestCase("True")]
        [TestCase("FALSE")]
        [TestCase("false")]
        [TestCase("False")]
        [TestCase("YES")]
        [TestCase("NO")]
        [TestCase("yes")]
        [TestCase("no")]
        [TestCase("1")]
        [TestCase("0")]
        public async Task RecognisePriorLearning_Field_Validation_Check(string flag)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = flag;

            var errors = await fixture.Handle();
            fixture.ValidateNoErrorsFound(errors);
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_IsBlank()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: null, priceReducedBy: 1);

            var errors = await fixture.Handle();
            fixture.ValidateNoErrorsFound(errors);
        }

        [Test]
        public async Task Prior_Learning_PriceReducedBy_IsBlank()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: null);

            var errors = await fixture.Handle();
            fixture.ValidateNoErrorsFound(errors);
        }
    }
}
