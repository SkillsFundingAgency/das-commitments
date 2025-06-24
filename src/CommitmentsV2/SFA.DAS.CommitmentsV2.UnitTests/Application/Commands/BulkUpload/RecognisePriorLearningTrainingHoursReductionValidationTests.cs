namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningTrainingHoursReductionValidationTests
    {
        [TestCase(0, "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more")]
        [TestCase(-10, "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more")]
        [TestCase(1000, "Total <b>reduction in off-the-job training time</b> due to RPL must be 999 hours or less")]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_AreInvalid(int trainingHoursReduction, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: 10, trainingTotalHours: 1000, trainingHoursReduction: trainingHoursReduction, isDurationReducedByRpl: true);

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Count.Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.Property == "TrainingHoursReduction" &&
                e.ErrorText == error).Should().Be(true);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Greater_Than_999()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("10000");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 999 hours or less");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Less_Than_1()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("0");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Is_Negative_Number()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("-10");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Multiple_Values()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("999 1234");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Alphanumeric()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("1234ABC");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_SpecialChar()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("#2342");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Values_TrainingTotalHours_and_TrainingHoursReduction_less_than_187()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("50");
            fixture.SetTrainingTotalHours("226");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 187 hours required for funding. Check if the <b>RPL reduction</b> is too high");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Values_TrainingTotalHours_and_TrainingHoursReduction_too_low()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("550");
            fixture.SetTrainingTotalHours("500");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be lower than the total off-the-job training time for this apprenticeship standard");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Uses_Course_Specific_Minimum_From_Dictionary()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("400");
            fixture.SetTrainingHoursReduction("150");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 300 hours required for funding. Check if the <b>RPL reduction</b> is too high");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Uses_Fallback_Minimum_When_Course_Not_In_Dictionary()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("300");
            fixture.SetTrainingHoursReduction("150");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "999", 500 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 187 hours required for funding. Check if the <b>RPL reduction</b> is too high");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Uses_Fallback_When_Course_Value_Is_Null()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("300");
            fixture.SetTrainingHoursReduction("150");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", null }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 187 hours required for funding. Check if the <b>RPL reduction</b> is too high");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Valid_With_Course_Specific_Minimum()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("400");
            fixture.SetTrainingHoursReduction("100");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateNoErrorsFound(errors);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Multiple_Courses_With_Different_Minimums()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("400");
            fixture.SetTrainingHoursReduction("250");
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 },
                { "123", 200 },
                { "456", 187 }
            };
            // Set the course code on the first csv record
            fixture.Command.CsvRecords.First().CourseCode = "59";

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 300 hours required for funding. Check if the <b>RPL reduction</b> is too high");
        }
    }
}
