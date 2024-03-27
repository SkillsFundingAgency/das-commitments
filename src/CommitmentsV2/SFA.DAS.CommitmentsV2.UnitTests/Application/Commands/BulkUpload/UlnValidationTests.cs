namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class UlnValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_IsNot_9999999999()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("9999999999");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "The <b>unique learner number</b> of 9999999999 isn't valid");
        }

        [Test]
        public async Task Validate_Is_LessThan_10()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("12345678901");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_A_Valid_ULN_Pattern()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("0112233669");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, false);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "The <b>start date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_EndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(false, true);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "The <b>end date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_When_Duplicate_ULN()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUpDuplicateUln();
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "Uln", "The <b>unique learner number</b> has already been used for an apprentice in this file");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_Start_And_EndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, true);
            var errors = await fixture.Handle();

            Assert.That(errors.BulkUploadValidationErrors, Has.Count.EqualTo(1));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].ErrorText, Is.EqualTo("The <b>start date</b> overlaps with existing training dates for the same apprentice"));
                Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].ErrorText, Is.EqualTo("The <b>end date</b> overlaps with existing training dates for the same apprentice"));
                Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].Property, Is.EqualTo("Uln"));
                Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].Property, Is.EqualTo("Uln"));
            });
        }
    }
}
