﻿namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningTrainingTotalHoursValidationTests
    {
        [TestCase(186, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more")]
        [TestCase(0, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more")]
        [TestCase(-10, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more")]
        [TestCase(10000, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less")]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_AreInvalid(int trainingTotalHours, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: 10, trainingTotalHours: trainingTotalHours, trainingHoursReduction: 10, isDurationReducedByRpl: true);

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Count.Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.Property == "TrainingTotalHours" &&
                e.ErrorText == error).Should().Be(true);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Greater_Than_9999()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("10001");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Negative_Number()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("-10");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Less_Than_278()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("34");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Not_Single_Value()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("1000 2000");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 187 and 9,999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_AlphaNumeric()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("6282ABC");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 187 and 9,999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_HasSpecialChar()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("#22738");

            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 187 and 9,999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Uses_Course_Specific_Minimum_From_Dictionary()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("250");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 300 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Uses_Fallback_When_Course_Not_In_Dictionary()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("180");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "123", 400 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Uses_Fallback_When_Course_Value_Is_Null()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("180");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", null }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 187 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Valid_With_Course_Specific_Minimum()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("350");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateNoErrorsFound(errors);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Multiple_Courses_With_Different_Minimums()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("250");
            
            fixture.Command.OtjTrainingHours = new Dictionary<string, int?>
            {
                { "59", 300 },
                { "123", 250 },
                { "456", 400 }
            };

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 300 hours or more");
        }
    }
}