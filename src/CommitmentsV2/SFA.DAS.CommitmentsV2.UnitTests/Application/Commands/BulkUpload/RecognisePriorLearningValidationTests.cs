namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningValidationTests
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

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
        }

        [Test]
        public async Task Prior_Learning_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: null);

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }

        [Test]
        public async Task RecognisePriorLearning_Field_Validation_Error()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = "XXX";

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }

        [TestCase("FALSE")]
        [TestCase("false")]
        [TestCase("False")]
        [TestCase("NO")]
        [TestCase("no")]
        [TestCase("0")]
        public async Task RecognisePriorLearning_Field_Validation_Check_When_False(string flag)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = flag;

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateNoErrorsFound(errors);
        }

        [TestCase("TRUE")]
        [TestCase("true")]
        [TestCase("True")]
        [TestCase("YES")]
        [TestCase("yes")]
        [TestCase("1")]
        public async Task RecognisePriorLearning_Field_Validation_Check_When_True_And_Companion_Fields_Present(string flag)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2022-08-01");
            fixture.CsvRecords[0].RecognisePriorLearningAsString = flag;
            fixture.SetValidRplCompanionFields();

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateNoErrorsFound(errors);
        }
    }
}
