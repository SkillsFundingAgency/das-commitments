using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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


    }
}
