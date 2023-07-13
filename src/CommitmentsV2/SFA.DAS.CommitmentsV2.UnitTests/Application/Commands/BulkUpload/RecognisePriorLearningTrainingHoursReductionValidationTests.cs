using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningTrainingHoursReductionValidationTests
    {
        [TestCase(0, "Total reduction in off-the-job training time due to RPL must be 1 hour or more")]
        [TestCase(-10, "Total reduction in off-the-job training time due to RPL must be 1 hour or more")]
        [TestCase(1000, "Total reduction in off-the-job training time due to RPL must be 999 hours or less")]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_AreInvalid(int trainingHoursReduction, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: 10, trainingTotalHours: 1000, trainingHoursReduction: trainingHoursReduction, isDurationReducedByRPL: true);

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.Property == "TrainingHoursReduction" &&
                e.ErrorText == error).Should().Be(true);
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Greater_Than_999()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("10000");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be 999 hours or less");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Less_Than_1()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("0");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be 1 hour or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Is_Negative_Number()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("-10");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be 1 hour or more");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Multiple_Values()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("999 1234");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999");
        }


        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Alphanumeric()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("1234ABC");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_SpecialChar()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("#2342");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Spaces()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction(" 2342");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Values_TrainingTotalHours_and_TrainingHoursReduction_less_than_278()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("50");
            fixture.SetTrainingTotalHours("300");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "TrainingHoursReduction", "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the RPL reduction is too high");
        }

        [Test]
        public async Task Prior_Learning_Training_When_TrainingHoursReduction_Values_TrainingTotalHours_and_TrainingHoursReduction_too_low()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetTrainingHoursReduction("550");
            fixture.SetTrainingTotalHours("500");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, "TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be lower than the total off-the-job training time for this apprenticeship standard");
        }
    }
}
