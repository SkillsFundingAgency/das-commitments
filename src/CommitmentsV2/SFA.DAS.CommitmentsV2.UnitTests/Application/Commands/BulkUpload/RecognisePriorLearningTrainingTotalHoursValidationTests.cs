using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningTrainingTotalHoursValidationTests
    {
        [TestCase(277, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more")]
        [TestCase(0, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more")]
        [TestCase(-10, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more")]
        [TestCase(10000, "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less")]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_AreInvalid(int trainingTotalHours, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: 10, trainingTotalHours: trainingTotalHours, trainingHoursReduction: 10, isDurationReducedByRpl: true);

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.Property == "TrainingTotalHours" &&
                e.ErrorText == error).Should().Be(true);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Greater_Than_9999()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("10001");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Negative_Number()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("-10");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Is_Less_Than_278()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("34");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_Not_Single_Value()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("“1000 2000");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 278 and 9,999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_AlphaNumeric()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("6282ABC");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 278 and 9,999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingTotalHours_HasSpecialChar()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingTotalHours("#22738");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 278 and 9,999");
        }
    }
}