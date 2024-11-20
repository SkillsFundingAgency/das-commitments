namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class RecognisePriorLearningPriceReducedByValidationTests
    {
        [TestCase("18001", "Total <b>price reduction</b> due to RPL must be 18,000 or less")]
        [TestCase("0", "Total <b>price reduction</b> due to RPL must be 100 pounds or more")]
        [TestCase("-10", "Total <b>price reduction</b> due to RPL must be 100 pounds or more")]
        [TestCase("XXX", "Total <b>price reduction</b> due to RPL must be a number between 100 and 18,000")]
        [TestCase("100 230", "Total <b>price reduction</b> due to RPL must be a number between 100 and 18,000")]
        [TestCase("6282ABC", "Total <b>price reduction</b> due to RPL must be a number between 100 and 18,000")]
        [TestCase("#22738", "Total <b>price reduction</b> due to RPL must be a number between 100 and 18,000")]
        public async Task When_PriceReducedBy_AreInvalid(string priceReducedBy, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetPriorLearningRaw(recognisePriorLearning: true, durationReducedByAsString: "100", 
                priceReducedByAsString: priceReducedBy, trainingTotalHoursAsString: "230",
                trainingHoursReductionAsString: "10", isDurationReducedByRplAsString: "true");

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Any(e =>
                e.Property == "PriceReducedBy" &&
                e.ErrorText == error).Should().Be(true);
        }

        [TestCase("18000")]
        [TestCase("10000")]
        [TestCase("100")]
        [TestCase("101")]
        public async Task When_PriceReducedBy_AreValid(string priceReducedBy)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetPriorLearningRaw(recognisePriorLearning: true, durationReducedByAsString: "100",
                priceReducedByAsString: priceReducedBy, trainingTotalHoursAsString: "230",
                trainingHoursReductionAsString: "10", isDurationReducedByRplAsString: "true");

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Any(e => e.Property == "PriceReducedBy").Should().Be(false);
        }

        [TestCase("XXX", "The <b>price this apprenticeship has been reduced by</b> due to prior learning should not be entered when recognise prior learning is false")]
        [TestCase("999", "The <b>price this apprenticeship has been reduced by</b> due to prior learning should not be entered when recognise prior learning is false")]
        public async Task When_PriceReducedBy_ArePresent_But_RPL_Is_False(string priceReducedBy, string error)
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetPriorLearningRaw(recognisePriorLearning: false, durationReducedByAsString: "100",
                priceReducedByAsString: priceReducedBy, trainingTotalHoursAsString: "230",
                trainingHoursReductionAsString: "10", isDurationReducedByRplAsString: "true");

            var errors = await fixture.Handle();
            var domainErrors = errors.BulkUploadValidationErrors.SelectMany(x => x.Errors).ToList();

            domainErrors.Any(e =>
                e.Property == "PriceReducedBy" &&
                e.ErrorText == error).Should().Be(true);
        }
    }
}