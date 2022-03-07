using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class AgreementIdValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "<b>Agreement ID</b> must be entered");
        }

        [Test]
        public async Task Validate_IsAllLetterOrDigit()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC*12");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsLessThan_6_Characters()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC1234");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsAValidEmployer()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC123");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsSigned()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetIsAgreementSigned(false);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "You cannot add apprentices for this employer as they need to <b>accept the agreement</b> with the ESFA.");
        }

        [Test]
        public async Task Validate_IsNotNonLevy()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetLevyStatus(Types.ApprenticeshipEmployerType.NonLevy);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "You cannot add apprentices via file on behalf of <b>non-levy employers</b> yet.");
        }
    }
}
