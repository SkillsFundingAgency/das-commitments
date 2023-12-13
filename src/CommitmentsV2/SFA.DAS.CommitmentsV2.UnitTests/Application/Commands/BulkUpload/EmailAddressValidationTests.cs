using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class EmailAddressValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEmailAddress("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "<b>Email address</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_A_Valid_EmailAddress()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEmailAddress("accnm.com");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "Enter a valid <b>email address</b>");
        }

        [Test]
        public async Task Validate_Is_Less_Than_200_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEmailAddress(
                 "abc012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890" +
                 "01234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890@email.com");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "Enter an <b>email address</b> that is not longer than 200 characters");
        }

        [Test]
        public async Task Validate_Overlapping_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingEmail(CommitmentsV2.Domain.Entities.OverlapStatus.OverlappingStartDate);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address");
        }

        [Test]
        public async Task Validate_Overlapping_EndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingEmail(CommitmentsV2.Domain.Entities.OverlapStatus.OverlappingEndDate);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address");
        }

        [Test]
        public async Task Validate_Overlapping_DateEmbrace()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingEmail(CommitmentsV2.Domain.Entities.OverlapStatus.DateEmbrace);

            var errors = await fixture.Handle();
            Assert.That(errors.BulkUploadValidationErrors.Count, Is.EqualTo(1));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors.Count, Is.EqualTo(2));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].ErrorText, Is.EqualTo("The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].ErrorText, Is.EqualTo("The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].Property, Is.EqualTo("EmailAddress"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].Property, Is.EqualTo("EmailAddress"));
        }

        [Test]
        public async Task Validate_When_Duplicate_Email()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUpDuplicateEmail();
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EmailAddress", "The <b>email address</b> has already been used for an apprentice in this file");
        }

        [Test]
        public async Task Validate_Overlapping_DateWithIn()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingEmail(CommitmentsV2.Domain.Entities.OverlapStatus.DateWithin);

            var errors = await fixture.Handle();
            Assert.That(errors.BulkUploadValidationErrors.Count, Is.EqualTo(1));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors.Count, Is.EqualTo(2));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].ErrorText, Is.EqualTo("The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].ErrorText, Is.EqualTo("The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].Property, Is.EqualTo("EmailAddress"));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[1].Property, Is.EqualTo("EmailAddress"));
        }
    }
}
