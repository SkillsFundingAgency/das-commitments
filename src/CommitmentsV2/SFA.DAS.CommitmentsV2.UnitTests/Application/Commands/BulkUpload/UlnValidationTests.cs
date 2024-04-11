using NUnit.Framework;
using System.Threading.Tasks;
using Moq;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class UlnValidationTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Validate_IsNotEmpty(string uln)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.MockUlnValidator.Setup(x => x.Validate(uln)).Returns(UlnValidationResult.IsEmptyUlnNumber);

            fixture.SetUln(uln);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [TestCase("5166282108")]
        [TestCase("9999999999")]
        public async Task Validate_IsNot_AValidUlnNumber(string uln)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.MockUlnValidator.Setup(x => x.Validate(uln)).Returns(UlnValidationResult.IsInvalidUln);
            fixture.SetUln(uln);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", $"The <b>unique learner number</b> of {uln} isn't valid");
        }

        [Test]
        public async Task Validate_Is_LessThan_10()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.MockUlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);
            fixture.SetUln("12345678901");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_A_Valid_ULN_Pattern()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.MockUlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);
            fixture.SetUln("0112233669");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, false);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "The <b>start date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_EndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(false, true);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "The <b>end date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_When_Duplicate_ULN()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUpDuplicateUln();
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "Uln", "The <b>unique learner number</b> has already been used for an apprentice in this file");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_Start_And_EndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, true);
            var errors = await fixture.Handle();

            Assert.AreEqual(1, errors.BulkUploadValidationErrors.Count);
            Assert.AreEqual(2, errors.BulkUploadValidationErrors[0].Errors.Count);
            Assert.AreEqual("The <b>start date</b> overlaps with existing training dates for the same apprentice", errors.BulkUploadValidationErrors[0].Errors[0].ErrorText);
            Assert.AreEqual("The <b>end date</b> overlaps with existing training dates for the same apprentice", errors.BulkUploadValidationErrors[0].Errors[1].ErrorText);
            Assert.AreEqual("Uln", errors.BulkUploadValidationErrors[0].Errors[0].Property);
            Assert.AreEqual("Uln", errors.BulkUploadValidationErrors[0].Errors[1].Property);
        }
    }
}
