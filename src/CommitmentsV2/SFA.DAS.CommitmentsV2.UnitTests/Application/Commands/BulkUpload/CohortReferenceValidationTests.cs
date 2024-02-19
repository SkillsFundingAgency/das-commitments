using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class CohortReferenceValidationTests
    {
        private BulkUploadValidateCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new BulkUploadValidateCommandHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Validate_IsAValidCohort()
        {
            //Arrange
            _fixture.SetCohortRef("ABC*12");            
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "Enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_IsLessThan_20_Characters()
        {
            //Arrange
            _fixture.SetCohortRef("123456789012345678901");
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "Enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_Cohort_AgreementId()
        {
            //Arrange
            _fixture.SetCohortRef("123456789012345678");
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "Enter a valid <b>Cohort Ref</b>");
        }

        [Test]
        public async Task Validate_WithParty_If_With_Employer()
        {
            //Arrange
            _fixture.SetWithParty(Types.Party.Employer);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort, as it is with the employer. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_WithParty_If_With_TransferSender()
        {
            //Arrange
            _fixture.SetWithParty(Types.Party.TransferSender);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort, as it is with the transfer sending employer. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_IsLinkedToChangeOfParty()
        {
            //Arrange
            _fixture.SetChangeOfParty();
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", "You cannot add apprentices to this cohort. You need to <b>add this learner to a different or new cohort.</b>");
        }

        [Test]
        public async Task Validate_When_Provider_Has_No_Permission_Create_Cohort_On_Employer_Behalf_Levy()
        {
            //Arrange
            _fixture.SetCohortRef("").SetProviderHasPermissionToCreateCohort(false);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRefPermission", "The <b>employer must give you permission</b> to add apprentices on their behalf");
        }

        [Test]
        public async Task Validate_When_Provider_Has_No_Permission_Create_Cohort_On_Employer_Behalf_Non_Levy()
        {
            //Arrange
            _fixture.SetLevyStatus(Types.ApprenticeshipEmployerType.NonLevy);
            _fixture.SetCohortRef("").SetProviderHasPermissionToCreateCohort(false);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRefPermission", "You do not have permission to <b>add apprentice records</b> for this employer, so you cannot <b>reserve funds</b> on their behalf");
        }

        [Test]
        public async Task Validate_When_Cohort_Has_Incomplete_Record()
        {
            //Arrange
            _fixture.SetUpIncompleteRecord();
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", $"You cannot add apprentices to {_fixture.CsvRecords[0].CohortRef}, as this cohort contains incomplete records. You need to <b>complete all details</b> before you can add into this cohort.");
        }

        [Test]
        public async Task Validate_When_Cohort_Has_Same_Uln()
        {
            //Arrange
            _fixture.SetUpDuplicateUlnWithinTheSameCohort();
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", $"The <b>unique learner number</b> has already been used for an apprentice in this cohort.");
        }

        [Test]
        public async Task Validate_When_Cohort_Has_Same_Email()
        {
            //Arrange
            _fixture.SetUpDuplicateEmailWithinTheSameCohort();
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", $"The <b>email address</b> has already been used for an apprentice in this cohort.");
        }

        [Test]
        public async Task Validate_When_Cohort_Has_Overlapping_Uln()
        {
            //Arrange
            _fixture.SetUpOverlappingUlnWithinTheSameCohort(true, false);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            Assert.That(errors.BulkUploadValidationErrors, Has.Count.EqualTo(1));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors, Has.Count.EqualTo(1));
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", $"You cannot add apprentices to {_fixture.CsvRecords[0].CohortRef}, as this cohort contains an overlapping training date. You need to <b>resolve any overlapping training date errors</b> before you can add into this cohort.");
        }

        [Test]
        public async Task Validate_When_Cohort_Has_Overlapping_Email()
        {
            //Arrange
            _fixture.SetOverlappingEmailWithinTheSameCohort(CommitmentsV2.Domain.Entities.OverlapStatus.OverlappingStartDate);
            //Act
            var errors = await _fixture.Handle();
            //Assert
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "CohortRef", $"You cannot add apprentices to {_fixture.CsvRecords[0].CohortRef} as it contains an overlapping email address. You need to <b>enter a unique email address</b> before you can add into this cohort.");
        }
    }
}
