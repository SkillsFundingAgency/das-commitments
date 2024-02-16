using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningDurationReducedByValidationTests
    {
        [Test]
        public async Task Prior_Learning_IsDurationReducedBy_when_IsDurationReducedBy_true_and_RecognisePriorLearning_false()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("false");
            fixture.SetIsDurationReducedByRpl("true");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "IsDurationReducedByRPL", "True or false should not be selected for <b>duration reduced</b> when recognise prior learning is false.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_greater_260()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("261");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be 260 weeks or less.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_less__than_1()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("0");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be 1 week or more.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_negative()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("-10");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be 1 week or more.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_data_lenght_greater_three()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("1000");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be 260 weeks or less.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_has_spaces()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("268 289");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be a number between 1 and 260.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_alphanumeric()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("567SGHAJ");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be a number between 1 and 260.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_true_and_DurationReducedBy_special_char()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("#123");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "<b>Reduction in duration</b> must be a number between 1 and 260.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_RecognisePriorLearning_true_and_IsDurationReducedByRPL_false_and_DurationReducedBy_has_value()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("false");
            fixture.SetDurationReducedBy("123");

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DurationReducedBy", "The <b>duration this apprenticeship has been reduced by</b> due to prior learning should not be entered when reduction of duration by RPL is false.");
        }

        [Test]
        public async Task Prior_Learning_DurationReducedBy_When_Correct_Values_Set_All_Is_Valid()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture(true);
            fixture.SetRecognisePriorLearning("true");
            fixture.SetIsDurationReducedByRpl("true");
            fixture.SetDurationReducedBy("123");

            var errors = await fixture.Handle();
            fixture.ValidateNoErrorsFound(errors);
        }

    }
}