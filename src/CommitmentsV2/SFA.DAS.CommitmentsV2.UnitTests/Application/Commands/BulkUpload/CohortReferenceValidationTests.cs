using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class CohortReferenceValidationTests
    {

        [Test]
        public async Task Validate_IsAValidCohort()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCohortRef("ABC*12");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You must enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_IsLessThan_20_Characters()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCohortRef("123456789012345678901");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You must enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_Cohort_AgreementId()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCohortRef("123456789012345678");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You must enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_WithParty_If_With_Employer()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetWithParty(Types.Party.Employer);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort, as it is with the employer. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_WithParty_If_With_TransferSender()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetWithParty(Types.Party.TransferSender);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort, as it is with the transfer sending employer. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_IsLinkedToChangeOfParty()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetChangeOfParty();
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_When_Provider_Has_No_Permission_Create_Cohort_On_Employer_Behalf()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCohortRef("").SetProviderHasPermissionToCreateCohort(false);

            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CohortRef", "The <b>employer must give you permission</b> to add apprentices on their behalf");
        }
    }
}
