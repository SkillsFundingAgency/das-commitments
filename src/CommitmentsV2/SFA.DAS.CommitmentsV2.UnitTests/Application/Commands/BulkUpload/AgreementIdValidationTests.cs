namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class AgreementIdValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "AgreementId", "<b>Agreement ID</b> must be entered");
        }

        [Test]
        public async Task Validate_IsAllLetterOrDigit()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC*12");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsLessThan_6_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC1234");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsAValidEmployer()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC123");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsSigned()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetIsAgreementSigned(false);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "LegalAgreementId", "You cannot add apprentices for this employer as they need to <b>accept the agreement</b> with the DfE.");
        }
    }
}
