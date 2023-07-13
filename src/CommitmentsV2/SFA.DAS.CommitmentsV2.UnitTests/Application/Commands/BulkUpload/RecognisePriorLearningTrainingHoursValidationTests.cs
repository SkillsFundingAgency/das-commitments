using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningTrainingHoursValidationTests
    {
        [Test]
        public async Task Prior_Learning_Training_Something()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetRplDataExtended(true);
            fixture.SetStartDate("2022-08-01");
            fixture.SetPriorLearning(recognisePriorLearning: true, durationReducedBy: 100, priceReducedBy: 10, trainingTotalHours: 10, trainingHoursReduction: 10, isDurationReducedByRPL: true );

            var errors = await fixture.Handle();
            fixture.ValidateNoErrorsFound(errors);

            fixture.ValidateError(errors, 2, "Traing", $"The <b>unique learner number</b> has already been used for an apprentice in this cohort.");

        }
    }
}
