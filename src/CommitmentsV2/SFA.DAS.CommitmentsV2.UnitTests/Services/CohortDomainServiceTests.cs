using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.Context;
using DateRange = SFA.DAS.CommitmentsV2.Domain.Entities.DateRange;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class CohortDomainServiceTests
{
    private CohortDomainServiceTestFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new CohortDomainServiceTestFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture.TearDown();
        _fixture = null;
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task CreateCohort_CreatingParty_Creates_Cohort(Party party)
    {
        await _fixture
            .WithParty(party)
            .CreateCohort();
        _fixture.VerifyCohortCreation(party);
    }

    [Test]
    public async Task CreateCohort_Employer_WithTransferSenderId_Creates_Cohort()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohort(null, null, _fixture.TransferSenderId, null);
        _fixture.VerifyCohortCreationWithTransferSender(Party.Employer, null);
    }

    [Test]
    public async Task CreateCohort_Employer_WithPledgeApplicationId_Creates_Cohort()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohort(null, null, _fixture.TransferSenderId, _fixture.PledgeApplicationId);
        _fixture.VerifyCohortCreationWithTransferSender(Party.Employer, _fixture.PledgeApplicationId);
    }

    [Test]
    public async Task CreateCohort_WithAnInvalidTransferSenderId_ThrowsBadRequestException()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohort(null, null, -1);

        _fixture.VerifyException<BadRequestException>();
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task CreateCohort_CreatingPartyWithoutTransferSenderId_Creates_Cohort(Party party)
    {
        await _fixture
            .WithParty(party)
            .CreateCohort(null, null, null);
        _fixture.VerifyCohortCreationWithoutTransferSender(party);
    }

    [Test]
    public async Task CreateCohortWithOtherParty_WithNoTransferSenderId_Creates_Cohort()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty();

        _fixture.VerifyCohortCreationWithOtherParty_WithoutTransferSender();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_WithTransferSenderId_Creates_Cohort()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty(_fixture.TransferSenderId);

        _fixture.VerifyCohortCreationWithOtherParty_WithTransferSender();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_WithPledgeApplicationId_Creates_Cohort()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty(_fixture.TransferSenderId, _fixture.PledgeApplicationId);

        _fixture.VerifyCohortCreationWithOtherParty_WithPledgeApplicationId();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_WithAnInvalidTransferSenderId_ThrowsBadRequestException()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty(-1);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_Creates_CohortWithoutAMessage()
    {
        await _fixture
            .WithParty(Party.Employer)
            .WithNoMessage()
            .CreateCohortWithOtherParty();

        _fixture.VerifyCohortCreationWithOtherParty_WithoutTransferSender();
    }

    [Test]
    public async Task CreateCohort_ThrowsBadRequest_WhenAccountLegalEntityNotFound()
    {
        await _fixture
            .CreateCohort(_fixture.AccountId, 2323);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohort_ThrowsBadRequest_WhenTransferSenderNotFound()
    {
        await _fixture
            .CreateCohort(null, null, -1);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_ThrowsBadRequest_WhenTransferSenderNotFound()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty(-1);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohort_ThrowsBadRequest_WhenPledgeApplicationNotFound()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohort(null, null, _fixture.TransferSenderId, _fixture.PledgeApplicationId + 1);

        _fixture.VerifyException<BadRequestException>();
    }

    [TestCase(PledgeApplication.ApplicationStatus.Declined)]
    [TestCase(PledgeApplication.ApplicationStatus.Pending)]
    [TestCase(PledgeApplication.ApplicationStatus.Rejected)]
    [TestCase(PledgeApplication.ApplicationStatus.Pending)]
    public async Task CreateCohort_ThrowsBadRequest_WhenPledgeApplication_Status_Is_Invalid(PledgeApplication.ApplicationStatus status)
    {
        await _fixture
            .WithParty(Party.Employer)
            .WithPledgeApplicationStatus(status)
            .CreateCohort(null, null, _fixture.TransferSenderId, _fixture.PledgeApplicationId);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohortWithOtherParty_ThrowsBadRequest_WhenPledgeApplicationNotFound()
    {
        await _fixture
            .WithParty(Party.Employer)
            .CreateCohortWithOtherParty(_fixture.TransferSenderId, _fixture.PledgeApplicationId + 1);

        _fixture.VerifyException<BadRequestException>();
    }

    [Test]
    public async Task CreateCohort_ThrowsBadRequest_WhenAccountIdDoesNotMatchAccountIdOnLegalEntity()
    {
        await _fixture
            .CreateCohort(4545, _fixture.AccountLegalEntityId);

        _fixture.VerifyException<BadRequestException>();
    }

    [TestCase(Party.Employer, false)]
    [TestCase(Party.Provider, true)]
    [TestCase(Party.TransferSender, true)]
    public async Task CreateCohortWithOtherParty_Throws_If_Not_Employer(Party creatingParty, bool expectThrows)
    {
        await _fixture
            .WithParty(creatingParty)
            .CreateCohortWithOtherParty();

        if (expectThrows)
        {
            _fixture.VerifyException<InvalidOperationException>();
        }
        else
        {
            _fixture.VerifyNoException();
        }
    }

    [Test]
    public async Task CreateEmptyCohort_Creates_EmptyCohort()
    {
        await _fixture
            .WithParty(Party.Provider)
            .CreateEmptyCohort();

        _fixture.VerifyEmptyCohortCreation(Party.Provider);
    }

    [TestCase(Party.Employer, true)]
    [TestCase(Party.Provider, false)]
    [TestCase(Party.TransferSender, true)]
    public async Task CreateEmptyCohort_Throws_If_Not_Provider(Party creatingParty, bool expectThrows)
    {
        await _fixture
            .WithParty(creatingParty)
            .CreateEmptyCohort();

        if (expectThrows)
        {
            _fixture.VerifyException<InvalidOperationException>();
        }
        else
        {
            _fixture.VerifyNoException();
        }
    }

    [Test]
    public async Task AddDraftApprenticeship_Provider_Adds_Draft_Apprenticeship()
    {
        _fixture.WithParty(Party.Provider).WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Provider);
        await _fixture.AddDraftApprenticeship();
        _fixture.VerifyProviderDraftApprenticeshipAdded();
    }

    [Test]
    public void AddDraftApprenticeship_CohortNotFound_ShouldThrowException()
    {
        Assert.ThrowsAsync<BadRequestException>(() => _fixture.AddDraftApprenticeship(false),
            $"Cohort {_fixture.CohortId} was not found");
    }

    [TestCase("2019-07-31", null, true)]
    [TestCase("2019-07-31", "2020-08-01", false, Description = "One day after cut off")]
    [TestCase("2019-07-31", "2020-07-31", true, Description = "Day of cut off (last valid day)")]
    [TestCase("2019-07-31", "2018-01-01", true, Description = "Day in the past")]
    public async Task StartDate_CheckIsWithinAYearOfEndOfCurrentTeachingYear_Validation(
        DateTime academicYearEndDate, DateTime? startDate, bool passes)
    {
        await _fixture
            .WithParty(Party.Provider)
            .WithAcademicYearEndDate(academicYearEndDate)
            .WithStartDate(startDate)
            .CreateCohort();

        _fixture.VerifyStartDateException(passes);
    }

    [TestCase(UlnValidationResult.IsEmptyUlnNumber, true)]
    [TestCase(UlnValidationResult.Success, true)]
    [TestCase(UlnValidationResult.IsInValidTenDigitUlnNumber, false)]
    [TestCase(UlnValidationResult.IsInvalidUln, false)]
    public async Task Uln_Validation(UlnValidationResult validationResult, bool passes)
    {
        await _fixture
            .WithParty(Party.Provider)
            .WithUlnValidationResult(validationResult)
            .CreateCohort();

        _fixture.VerifyUlnException(passes);
    }

    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    public async Task Reservation_Validation(bool hasValidationError, bool passes, bool usingActualStartDate)
    {
        await _fixture
            .WithParty(Party.Provider)
            .WithReservationValidationResult(hasValidationError, usingActualStartDate)
            .CreateCohort();

        _fixture.VerifyReservationException(passes);
    }

    [Test]
    public async Task Reservation_Validation_Skipped()
    {
        await _fixture.WithParty(Party.Provider).CreateCohort();
        _fixture.VerifyReservationValidationNotPerformed();
    }

    [TestCase(Party.Provider, "employer")]
    [TestCase(Party.Employer, "provider")]
    public async Task CreateCohort_OverlapOnStartDate_Validation(Party party, string otherPartyInMessage)
    {
        await _fixture.WithParty(party).WithUlnOverlapOnStartDate().CreateCohort();
        _fixture.VerifyOverlapExceptionOnStartDate(otherPartyInMessage);
    }

    [TestCase(Party.Provider, "employer")]
    [TestCase(Party.Employer, "provider")]
    public async Task CreateCohort_OverlapOnActualStartDate_Validation(Party party, string otherPartyInMessage)
    {
        await _fixture.WithParty(party).WithUlnOverlapOnActualStartDate().CreateCohort();
        _fixture.VerifyOverlapExceptionOnActualStartDate(otherPartyInMessage);
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task CreateCohort_OverlapOnStartDate_Validation_WithIgnore(Party party)
    {
        await _fixture
            .WithParty(party)
            .WithUlnOverlapOnStartDate()
            .CreateCohort(ignoreStartDateOverlap: true);

        _fixture.VerifyNoUlnOverlaps();
    }

    [TestCase(Party.Provider, "employer")]
    [TestCase(Party.Employer, "provider")]
    public async Task CreateCohort_OverlapOnEndDate_Validation(Party party, string otherPartyInMessage)
    {
        await _fixture.WithParty(party).WithUlnOverlapOnEndDate().CreateCohort();
        _fixture.VerifyOverlapExceptionOnEndDate(otherPartyInMessage);
    }

    [TestCase(Party.Provider, "employer", true)]
    [TestCase(Party.Provider, "employer", false)]
    [TestCase(Party.Employer, "provider", true)]
    [TestCase(Party.Employer, "provider", false)]
    public async Task CreateCohort_OverlapOnStartDateAndEndDateEmbraceWithin_Validation(Party party, string otherPartyInMessage, bool ignoreStartDateOverlap)
    {
        await _fixture
            .WithParty(party)
            .WithUlnOverlapOnStartAndEndDate()
            .CreateCohort(ignoreStartDateOverlap: ignoreStartDateOverlap);

        _fixture.VerifyOverlapExceptionOnEndDate(otherPartyInMessage);
        _fixture.VerifyOverlapExceptionOnStartDate(otherPartyInMessage);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Provider_AddDraftApprenticeship_OverlapOnStartDateAndEndDateEmbraceWithin_Validation(bool ignoreStartDateOverlap)
    {
        await _fixture
            .WithParty(Party.Provider)
            .WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Provider)
            .WithUlnOverlapOnStartAndEndDate()
            .AddDraftApprenticeship(ignoreStartDateOverlap: ignoreStartDateOverlap);

        _fixture.VerifyOverlapExceptionOnEndDate("employer");
        _fixture.VerifyOverlapExceptionOnStartDate("employer");
    }

    [TestCase(Party.Provider, true)]
    [TestCase(Party.Provider, false)]
    [TestCase(Party.Employer, true)]
    [TestCase(Party.Employer, false)]
    public async Task EmailOverlapOnApprenticeship_Validation(Party party, bool isApproved)
    {
        await _fixture.WithParty(party).WithEmailOverlapWithApprenticeship(isApproved).CreateCohort();
        _fixture.VerifyEmailOverlapExceptionOnApprenticeship(isApproved);
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task EmailOverlapOnApprenticeship_Validation_FindsNoOverlaps(Party party)
    {
        await _fixture.WithParty(party).WithNoEmailOverlaps().CreateCohort();
        _fixture.VerifyCheckForEmailOverlapsIsCalledCorrectlyWhenCreatingCohortWithInitialApprenticeship();
    }

    [TestCase(Party.Provider, true)]
    [TestCase(Party.Provider, false)]
    [TestCase(Party.Employer, true)]
    [TestCase(Party.Employer, false)]
    public async Task ActualStartDateEmailOverlapOnApprenticeship_Validation(Party party, bool isApproved)
    {
        await _fixture.WithParty(party).WithActualStartDateEmailOverlapWithApprenticeship(isApproved).CreateCohort();
        _fixture.VerifyEmailOverlapExceptionOnApprenticeship(isApproved);
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task ActualStartDateEmailOverlapOnApprenticeship_Validation_FindsNoOverlaps(Party party)
    {
        await _fixture.WithParty(party).WithNoActualStartDateEmailOverlaps().CreateCohort();
        _fixture.VerifyCheckForActualStartDateEmailOverlapsIsCalledCorrectlyWhenCreatingCohortWithInitialApprenticeship();
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task UpdateDraftApprenticeship_IsSuccessful_ThenDraftApprenticeshipIsUpdated(Party withParty)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeship();
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyDraftApprenticeshipUpdated();
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.Employer)]
    public async Task UpdateDraftApprenticeship_WhenUserInfoDoesExist_ThenLastUpdatedFieldsAreSet(Party withParty)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeship();
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyLastUpdatedFieldsAreSet(withParty);
    }

    [Test]
    public async Task UpdateDraftApprenticeship_WhenUserInfoDoesNotExist_ThenLastUpdatedFieldsAreNotSet()
    {
        _fixture.WithParty(Party.Employer).WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer).WithExistingDraftApprenticeship().WithNoUserInfo();
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyLastUpdatedFieldsAreNotSet();
    }

        [Test]
        public void AddDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<CohortAlreadyApprovedException>(() => _fixture.AddDraftApprenticeship());
        }
        
        [Test]
        public void UpdateDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<CohortAlreadyApprovedException>(() => _fixture.UpdateDraftApprenticeship());
        }

        [Test]
        public void SendCohortToOtherParty_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<CohortAlreadyApprovedException>(() => _fixture.SendCohortToOtherParty());
        }

        [Test]
        public void ApproveCohort_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<CohortAlreadyApprovedException>(() => _fixture.ApproveCohort());
        }

        [Test]
        public void ApproveCohort_WhenEmployerApprovesAndAgreementIsNotSigned_ShouldThrowException()
        {
            _fixture.WithParty(Party.Employer).WithExistingUnapprovedCohort().WithDecodeOfPublicHashedAccountLegalEntity().WithAgreementSignedAs(false);
            Assert.ThrowsAsync<CohortAlreadyApprovedException>(() => _fixture.ApproveCohort());
        }

    [Test]
    [Ignore("Test is faulty. No setup is provided for getting ALE as required. Previous test passed due to expecting InvalidOperationExeption, the condition for which was met but not by the right error in the right place.")]
    public void ApproveCohort_WhenEmployerApprovesAndThereIsATransferSenderAndAgreementIsNotSigned_ShouldThrowException()
    {
        _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort();
        Assert.ThrowsAsync<DomainException>(() => _fixture.ApproveCohort());
    }

    [TestCase(Party.Employer)]
    [TestCase(Party.Provider)]
    public async Task ApproveCohort_WhenThereIsAOverlap_ShouldThrowException(Party party)
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Provider).WithParty(party).WithExistingDraftApprenticeship().WithUlnOverlap(true);

        await _fixture.ApproveCohort();

        _fixture.VerifyException<DomainException>();
    }

    [Test]
    public async Task ApproveCohort_WhenEmployerApprovesAndAgreementIsSignedAndNoEmailOverlaps_ShouldSucceed()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithAgreementSignedAs(true)
            .WithExistingDraftApprenticeship()
            .WithUlnOverlap(false);

        await _fixture.WithParty(Party.Employer).ApproveCohort();
        _fixture.VerifyIsAgreementSignedIsCalledCorrectly();
        _fixture.VerifyCheckForEmailOverlapsOnCohortIsCalledCorrectlyWhenApproving();
    }

    [Test]
    public async Task ApproveCohort_WhenEmployerApprovesAndAgreementIsSignedButHasEmailOverlaps_ShouldThrowException()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithAgreementSignedAs(true)
            .WithExistingDraftApprenticeship()
            .WithOverlappingEmails();

        await _fixture.WithParty(Party.Employer).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Has.Count.EqualTo(1));
        Assert.That(_fixture.DomainErrors[0].ErrorMessage, Is.EqualTo("Cannot approve this cohort because one or more emails are failing the overlap check"));
    }

    [Test]
    public async Task ApproveCohort_WhenRPLIsRequiredButNoRPLData_ShouldThrowException()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship();

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Has.Count.EqualTo(1));
        Assert.That(_fixture.DomainErrors[0].ErrorMessage, Is.EqualTo("Cohort must be complete for Provider"));
    }

    [Test]
    public async Task ApproveCohort_WhenRPLIsRequiredAndRPLDataIsPresent_ShouldSucceed()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship()
            .WithPriorLearningData();

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Is.Empty);
    }

    [Test]
    [Ignore("Until RPL reduction is raised to error level again")]
    public async Task ApproveCohort_WhenRPLIsRequiredAndRPLDataIsPresentButRplReductionIsInError_ShouldFail()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship()
            .WithPriorLearningData()
            .WithRplPriceReductionError();

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ApproveCohort_WhenExtendedRPLIsRequiredAndRPLDataIsPresent_ShouldSucceed()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship()
            .WithPriorLearningExtended();

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Is.Empty);
    }

    [Test]
    public async Task ApproveCohort_WhenRPLIsRequiredAndExtendedRPLDataIsPresent_ShouldSucceed()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship()
            .WithExtendedPriorLearning(); ;

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Is.Empty);
    }

    [Test]
    public async Task ApproveCohort_WhenRPLDataIsRequiredAndRPLDataIsPresent_ShouldSucceed()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Provider, Party.Provider)
            .WithDecodeOfPublicHashedAccountLegalEntity()
            .WithExistingDraftApprenticeship()
            .WithPriorLearningData();

        await _fixture.WithParty(Party.Provider).ApproveCohort();

        Assert.That(_fixture.DomainErrors, Is.Empty);
    }

    [Test]
    public async Task DeleteDraftApprenticeship_WhenCohortIsWithEmployer()
    {
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer).WithExistingDraftApprenticeship();
        await _fixture.WithParty(Party.Employer).DeleteDraftApprenticeship();
        _fixture.VerifyDraftApprenticeshipDeleted();
    }

    [Test]
    public async Task DeleteFlexiJobDraftApprenticeship_WhenCohortIsWithEmployer()
    {
        _fixture.ExistingDraftApprenticeship.FlexibleEmployment = new FlexibleEmployment { EmploymentPrice = 10, EmploymentEndDate = DateTime.Now };
        _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer).WithExistingDraftApprenticeship();
        await _fixture.WithParty(Party.Employer).DeleteDraftApprenticeship();
        _fixture.VerifyDraftApprenticeshipDeleted();
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task UpdateDraftApprenticeship_WhenContinuation_StartDateMustBeAfterPreviousStopDate(bool overlap, bool expectThrow)
    {
        _fixture.WithParty(Party.Employer)
            .WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer)
            .WithExistingDraftApprenticeship()
            .WithContinuation(overlap);
        
        await _fixture.UpdateDraftApprenticeship();

        if (expectThrow)
        {
            _fixture.VerifyException<DomainException>();
        }
        else
        {
            _fixture.VerifyNoException();
        }
    }

    [TestCase("2018-04-30", false)]
    [TestCase("2018-05-01", true)]
    public async Task AddDraftApprenticeship_Verify_StartDate_ForTransferSender_Is_After_May_2018(DateTime startDate, bool pass)
    {
        _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort()
            .WithStartDate(startDate)
            .WithTrainingProgramme();

        await _fixture.AddDraftApprenticeship();

        _fixture.VerifyStartDateException(pass);
    }

    [TestCase("0022-01-01", false)]
    [TestCase("1300-01-01", false)]
    [TestCase("2000-12-01", false)]
    public async Task AddDraftApprenticeship_Verify_StartDate_IsNot_Earlier_Than_May_2017(DateTime startDate, bool pass)
    {
        _fixture.WithParty(Party.Employer)
            .WithStartDate(startDate)
            .WithTrainingProgramme();

        await _fixture.CreateCohort();

        _fixture.VerifyStartDateException(pass);
    }

    [TestCase("2018-04-30", false)]
    [TestCase("2018-05-01", true)]
    public async Task AddDraftApprenticeship_Verify_ActualStartDate_ForTransferSender_Is_After_May_2018(DateTime startDate, bool pass)
    {
        _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort()
            .WithActualStartDate(startDate)
            .WithTrainingProgramme();

        await _fixture.AddDraftApprenticeship();

        _fixture.VerifyActualStartDateException(pass);
    }

    [TestCase("0022-01-01", false)]
    [TestCase("1300-01-01", false)]
    [TestCase("2000-12-01", false)]
    public async Task AddDraftApprenticeship_Verify_ActualStartDate_IsNot_Earlier_Than_May_2017(DateTime startDate, bool pass)
    {
        _fixture.WithParty(Party.Employer)
            .WithActualStartDate(startDate)
            .WithTrainingProgramme();

        await _fixture.CreateCohort();

        _fixture.VerifyActualStartDateException(pass);
    }

    [TestCase("0022-01-01", false)]
    [TestCase("1300-01-01", false)]
    [TestCase("2000-12-01", false)]
    public async Task AddDraftApprenticeship_Verify_EndDate_IsNot_Earlier_Than_May_2017(DateTime endDate, bool pass)
    {
        _fixture.WithParty(Party.Employer)
            .WithEndDate(endDate)
            .WithStartDate(new DateTime(DateTime.UtcNow.Year,02, 02))
            .WithTrainingProgramme();

        await _fixture.CreateCohort();

        _fixture.VerifyEndDateException(pass);
    }

    [TestCase(ProgrammeType.Framework, false)]
    [TestCase(ProgrammeType.Standard, true)]
    public async Task AddDraftApprenticeship_Verify_For_TransferSender_Framework_Course_Are_Not_Available(ProgrammeType programmeType, bool passes)
    {
        _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort()
            .WithTrainingProgramme(programmeType);

        await _fixture.AddDraftApprenticeship();

        _fixture.VerifyCourseException(passes);
    }

    [TestCase(Party.Employer, "2022-07-31")]
    [TestCase(Party.Provider, "2022-07-31")]
    [TestCase(Party.Employer, "2022-04-01")]
    [TestCase(Party.Provider, "2022-04-01")]
    public async Task UpdateDraftApprenticeship_IsSuccessfulAndStartDateIsPriorToAug2022_ThenDraftApprenticeshipIsUpdatedButPriorLearningDataIsRemoved(Party withParty, DateTime startDate)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeshipWithPriorLearning();
        _fixture.DraftApprenticeshipDetails.StartDate = startDate;
        _fixture.DraftApprenticeshipDetails.EndDate = startDate.AddYears(1);
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyPriorLearningIsNull();
    }

    [TestCase(Party.Employer, "2022-08-01")]
    [TestCase(Party.Provider, "2022-08-01")]
    [TestCase(Party.Employer, "2022-09-01")]
    [TestCase(Party.Provider, "2022-09-01")]
    public async Task UpdateDraftApprenticeship_IsSuccessfulAndStartDateIsAfterToAug2022_ThenDraftApprenticeshipIsUpdatedButPriorLearningDataIsStillPresent(Party withParty, DateTime startDate)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeshipWithPriorLearning();
        _fixture.DraftApprenticeshipDetails.StartDate = startDate;
        _fixture.DraftApprenticeshipDetails.EndDate = startDate.AddYears(1);
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyPriorLearningIsStillPresent();
    }

    [TestCase(Party.Employer, "2022-08-01")]
    [TestCase(Party.Provider, "2022-08-01")]
    public async Task UpdateDraftApprenticeship_IsSuccessfulAndRPLHasPreviouslyBeenSet_ThenDraftApprenticeshipIsUpdatedButPriorLearningDataNotAffected(Party withParty, DateTime startDate)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeshipWithPriorLearning().WithNewRplDetails();
        _fixture.DraftApprenticeshipDetails.StartDate = startDate;
        _fixture.DraftApprenticeshipDetails.EndDate = startDate.AddYears(1);
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyPriorLearningIsNotSetToNewRPLValues();
    }

    [TestCase(Party.Employer, "2022-08-01")]
    [TestCase(Party.Provider, "2022-08-01")]
    public async Task UpdateDraftApprenticeship_IsSuccessfulAndRPLHasNotPreviouslyBeenSet_ThenDraftApprenticeshipIsUpdatedAndPriorLearningDataIsAdded(Party withParty, DateTime startDate)
    {
        _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeship().WithNewRplDetails();
        _fixture.DraftApprenticeshipDetails.StartDate = startDate;
        _fixture.DraftApprenticeshipDetails.EndDate = startDate.AddYears(1);
        await _fixture.UpdateDraftApprenticeship();
        _fixture.VerifyPriorLearningIsNotSetToNewRPLValues();
    }

    public class CohortDomainServiceTestFixture
    {
        public DateTime ReferenceDate { get; set; }
        public CohortDomainService CohortDomainService { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public long ProviderId { get; }
        public long AccountId { get; }
        public long TransferSenderId { get; }
        public string TransferSenderName { get; }
        public int? PledgeApplicationId { get; }
        public long AccountLegalEntityId { get; }
        public long CohortId { get; }
        public Party RequestingParty { get; private set; }
        public string AccountLegalEntityPublicHashedId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
        public DraftApprenticeship ExistingDraftApprenticeship { get; }
        public Apprenticeship PreviousApprenticeship { get; }
        public long DraftApprenticeshipId { get; }
        public Mock<ChangeOfPartyRequest> ChangeOfPartyRequest { get; }
        public Account EmployerAccount { get; set; }
        public Account TransferSenderAccount { get; set; }
        public Mock<Provider> Provider { get; set; }
        public Mock<AccountLegalEntity> AccountLegalEntity { get; set; }
        public Cohort Cohort { get; set; }
        public Cohort NewCohort { get; set; }
        public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; }
        public Mock<IUlnValidator> UlnValidator { get; }
        public Mock<IReservationValidationService> ReservationValidationService { get; }
        public Mock<IEmployerAgreementService> EmployerAgreementService { get; }
        public Mock<IEncodingService> EncodingService { get; }
        private Mock<IOverlapCheckService> OverlapCheckService { get; }
        private Mock<IEmailOptionalService> EmailOptionalService { get; }
        public Party Party { get; set; }
        public Mock<IAuthenticationService> AuthenticationService { get; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public Mock<IAccountApiClient> AccountApiClient { get; set; }
        public Mock<ILevyTransferMatchingApiClient> LevyTransferMatchingApiClient { get; set; }
        public PledgeApplication PledgeApplication { get; set; }
        public List<TransferConnectionViewModel> TransferConnections { get; }

        public Exception Exception { get; private set; }
        public List<DomainError> DomainErrors { get; }
        public string Message { get; private set; }
        public UserInfo UserInfo { get; private set; }
        public ApprenticeshipPriorLearning PriorLearning { get; private set; }
        public Mock<IRplFundingCalculationService> RplFundingCalculationService { get; set; }
        public long MaLegalEntityId { get; private set; }

        public CohortDomainServiceTestFixture()
        {
            ReferenceDate = new DateTime(DateTime.UtcNow.Year, 02, 02);
            var fixture = new Fixture();

            // We need this to allow the UoW to initialise it's internal static events collection.
            var uow = new UnitOfWorkContext();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options);

            ProviderId = 1;
            AccountId = 2;
            AccountLegalEntityId = 3;
            CohortId = 4;
            RequestingParty = Party.Employer;
            ChangeOfPartyRequestId = 5;
            MaLegalEntityId = fixture.Create<long>();
            AccountLegalEntityPublicHashedId = fixture.Create<string>();

            Message = fixture.Create<string>();

            NewCohort = new Cohort { Apprenticeships = new List<ApprenticeshipBase> { new DraftApprenticeship() } };

            Provider = new Mock<Provider>(() => new Provider(ProviderId, "Test Provider", DateTime.UtcNow, DateTime.UtcNow));
            Provider.Setup(x => x.CreateCohort(It.IsAny<long>(), It.IsAny<AccountLegalEntity>(), It.IsAny<UserInfo>()))
                .Returns(NewCohort);
            Db.Providers.Add(Provider.Object);

            EmployerAccount = new Account(AccountId, "AAAA", "BBBB", "Account 1", DateTime.UtcNow);
            Db.Accounts.Add(EmployerAccount);
            AccountLegalEntity = new Mock<AccountLegalEntity>(() =>
                new AccountLegalEntity(EmployerAccount, AccountLegalEntityId, MaLegalEntityId, "test", "ABC", "Test", OrganisationType.CompaniesHouse, "test", DateTime.UtcNow));
            AccountLegalEntity.Setup(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), null, null,
                    It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(NewCohort);
            AccountLegalEntity.Setup(x => x.CreateCohortWithOtherParty(ProviderId, It.IsAny<AccountLegalEntity>(), null, null,
                    It.IsAny<string>(), It.IsAny<UserInfo>()))
                .Returns(NewCohort);

            AccountLegalEntity.Setup(x => x.Account).Returns(EmployerAccount);
            AccountLegalEntity.Setup(x => x.Cohorts).Returns(new List<Cohort>());

            Db.AccountLegalEntities.Add(AccountLegalEntity.Object);

            TransferSenderId = 23;
            TransferSenderName = fixture.Create<string>();
            TransferSenderAccount = new Account(TransferSenderId, "XXXX", "ZZZZ", TransferSenderName, new DateTime());
            Db.Accounts.Add(TransferSenderAccount);

            PledgeApplicationId = fixture.Create<int>();
            PledgeApplication = new PledgeApplication
            {
                ReceiverEmployerAccountId = AccountId,
                SenderEmployerAccountId = TransferSenderId,
                Status = PledgeApplication.ApplicationStatus.Accepted
            };

            LevyTransferMatchingApiClient = new Mock<ILevyTransferMatchingApiClient>();
            LevyTransferMatchingApiClient.Setup(x => x.GetPledgeApplication(PledgeApplicationId.Value))
                .ReturnsAsync(PledgeApplication);

            TransferConnections = [new TransferConnectionViewModel { FundingEmployerAccountId = TransferSenderId }];


            DraftApprenticeshipId = fixture.Create<long>();

            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                FirstName = "Test",
                LastName = "Test",
                DeliveryModel = DeliveryModel.Regular,
                IgnoreStartDateOverlap = false
            };

            var referenceDate = new DateTime(DateTime.Now.Year, 02, 03);
            ExistingDraftApprenticeship = new DraftApprenticeship
            {
                Id = DraftApprenticeshipId,
                CommitmentId = CohortId,
                FirstName = fixture.Create<string>(),
                LastName = fixture.Create<string>(),
                Uln = "4860364820",
                StartDate = referenceDate,
                EndDate = referenceDate.AddYears(1),
                CourseCode = fixture.Create<string>(),
                Cost = fixture.Create<int>()
            };
            ExistingDraftApprenticeship.SetValue(x => x.DateOfBirth, ExistingDraftApprenticeship.StartDate.Value.AddYears(-16));

            PreviousApprenticeship = new Apprenticeship();
            PreviousApprenticeship.SetValue(x => x.Id, fixture.Create<long>());
            PreviousApprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = new AccountLegalEntity()
            });
            Db.Apprenticeships.Add(PreviousApprenticeship);

            ChangeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
            ChangeOfPartyRequest.Setup(x => x.Id).Returns(ChangeOfPartyRequestId);

            Db.ChangeOfPartyRequests.Add(ChangeOfPartyRequest.Object);

            var academicYear = ExistingDraftApprenticeship.StartDate.Value.Month > 7 ? ExistingDraftApprenticeship.StartDate.Value.Year : ExistingDraftApprenticeship.StartDate.Value.Year - 1;
            AcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
            AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(academicYear, 7, 31));

            UlnValidator = new Mock<IUlnValidator>();
            UlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.Success);

            ReservationValidationService = new Mock<IReservationValidationService>();
            ReservationValidationService.Setup(x =>
                    x.Validate(It.IsAny<ReservationValidationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ReservationValidationResult(Array.Empty<ReservationValidationError>()));

            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OverlapCheckResult(false, false));
            OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            EmployerAgreementService = new Mock<IEmployerAgreementService>();
            EncodingService = new Mock<IEncodingService>();

            AuthenticationService = new Mock<IAuthenticationService>();

            CurrentDateTime = new Mock<ICurrentDateTime>();
            CurrentDateTime.Setup(d => d.UtcNow).Returns(ReferenceDate);

            AccountApiClient = new Mock<IAccountApiClient>();
            AccountApiClient.Setup(x => x.GetTransferConnections(It.IsAny<string>()))
                .ReturnsAsync(TransferConnections);

            EmailOptionalService = new Mock<IEmailOptionalService>();

            PriorLearning = fixture.Create<ApprenticeshipPriorLearning>();

            RplFundingCalculationService = new Mock<IRplFundingCalculationService>();
            WithNoRplPriceReductionError();

            Exception = null;
            DomainErrors = [];
            UserInfo = fixture.Create<UserInfo>();

            CohortDomainService = new CohortDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<CohortDomainService>>(),
                AcademicYearDateProvider.Object,
                UlnValidator.Object,
                ReservationValidationService.Object,
                OverlapCheckService.Object,
                AuthenticationService.Object,
                CurrentDateTime.Object,
                EmployerAgreementService.Object,
                EncodingService.Object,
                AccountApiClient.Object,
                EmailOptionalService.Object,
                LevyTransferMatchingApiClient.Object);

            Db.SaveChanges();
        }

        public CohortDomainServiceTestFixture WithExtendedPriorLearning()
        {
            ExistingDraftApprenticeship.SetValue(x => x.RecognisePriorLearning, true);
            ExistingDraftApprenticeship.SetPriorLearningData(2000, 100, true, 12, 1000, 5, 9999, 187);
            return this;
        }

        public CohortDomainServiceTestFixture WithPriorLearningExtended()
        {
            ExistingDraftApprenticeship.SetValue(x => x.RecognisePriorLearning, true);
            ExistingDraftApprenticeship.SetPriorLearningData(2000, 100, true, 20, 110, 5, 9999, 187);
            return this;
        }

        public CohortDomainServiceTestFixture WithNoRplPriceReductionError()
        {
            var rplReductionCalc = new RplFundingCalculation { RplPriceReductionError = false };
            RplFundingCalculationService.Setup(x => x.GetRplFundingCalculations
            (It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<DbSet<StandardFundingPeriod>>(),
                It.IsAny<DbSet<FrameworkFundingPeriod>>())).ReturnsAsync(rplReductionCalc);
            return this;
        }

        public CohortDomainServiceTestFixture WithRplPriceReductionError()
        {
            var rplReductionCalc = new RplFundingCalculation { RplPriceReductionError = true };
            RplFundingCalculationService.Setup(x => x.GetRplFundingCalculations
            (It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<DbSet<StandardFundingPeriod>>(),
                It.IsAny<DbSet<FrameworkFundingPeriod>>())).ReturnsAsync(rplReductionCalc);
            return this;
        }

        public CohortDomainServiceTestFixture WithPriorLearningData()
        {
            ExistingDraftApprenticeship.SetValue(x => x.RecognisePriorLearning, true);
            ExistingDraftApprenticeship.SetPriorLearningData(1000, 10, true, 5, 100, 5, 9999, 187);

            return this;
        }

        public CohortDomainServiceTestFixture WithAcademicYearEndDate(DateTime value)
        {
            var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(utcValue);
            return this;
        }

        public CohortDomainServiceTestFixture WithUlnValidationResult(UlnValidationResult value)
        {
            DraftApprenticeshipDetails.Uln = "X";
            UlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(value);
            return this;
        }

        public CohortDomainServiceTestFixture WithNoMessage()
        {
            Message = null;
            return this;
        }

        public CohortDomainServiceTestFixture WithTrainingProgramme(ProgrammeType programmeType = ProgrammeType.Standard)
        {
            DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme("TEST",
                "TEST",
                programmeType,
                new DateTime(2016, 1, 1),
                null);
            DraftApprenticeshipDetails.DeliveryModel = DeliveryModel.Regular;
            return this;
        }

        public CohortDomainServiceTestFixture WithNewRplDetails()
        {
            var fixture = new Fixture();
            DraftApprenticeshipDetails.DurationReducedBy = fixture.Create<int>();
            DraftApprenticeshipDetails.PriceReducedBy = fixture.Create<int>();
            DraftApprenticeshipDetails.DurationReducedByHours = fixture.Create<int>();
            DraftApprenticeshipDetails.RecognisePriorLearning = true;
            return this;
        }

        public CohortDomainServiceTestFixture WithReservationValidationResult(bool hasReservationError, bool usingActualStartDate = false)
        {
            DraftApprenticeshipDetails.ReservationId = Guid.NewGuid();
            if (usingActualStartDate)
            {
                DraftApprenticeshipDetails.ActualStartDate = new DateTime(2019, 01, 01);
            }
            else
            {
                DraftApprenticeshipDetails.StartDate = new DateTime(2019, 01, 01);
            }
            DraftApprenticeshipDetails.TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST",
                "TEST",
                ProgrammeType.Standard,
                new DateTime(2016, 1, 1),
                null);

            var errors = new List<ReservationValidationError>();

            if (hasReservationError)
            {
                errors.Add(new ReservationValidationError("TEST", "TEST"));
            }

            ReservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ReservationValidationResult(errors.ToArray()));

            return this;
        }

        public CohortDomainServiceTestFixture WithUlnOverlapOnStartDate()
        {
            DraftApprenticeshipDetails.Uln = "X";
            DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new OverlapCheckResult(true, false));

            return this;
        }

        public CohortDomainServiceTestFixture WithUlnOverlapOnActualStartDate()
        {
            DraftApprenticeshipDetails.Uln = "X";
            DraftApprenticeshipDetails.ActualStartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new OverlapCheckResult(true, false));

            return this;
        }

        public CohortDomainServiceTestFixture WithUlnOverlapOnEndDate()
        {
            DraftApprenticeshipDetails.Uln = "X";
            DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new OverlapCheckResult(false, true));

            return this;
        }

        public CohortDomainServiceTestFixture WithUlnOverlapOnStartAndEndDate()
        {
            DraftApprenticeshipDetails.Uln = "X";
            DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == DraftApprenticeshipDetails.Uln), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new OverlapCheckResult(true, true));

            return this;
        }

        public CohortDomainServiceTestFixture WithEmailOverlapWithApprenticeship(bool isApproved)
        {
            DraftApprenticeshipDetails.Email = "test@test.com";
            DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailOverlapCheckResult(1, OverlapStatus.OverlappingEndDate, isApproved));

            return this;
        }

        public CohortDomainServiceTestFixture WithNoEmailOverlaps()
        {
            DraftApprenticeshipDetails.Email = "test@test.com";
            DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            return this;
        }

        public CohortDomainServiceTestFixture WithActualStartDateEmailOverlapWithApprenticeship(bool isApproved)
        {
            DraftApprenticeshipDetails.Email = "test@test.com";
            DraftApprenticeshipDetails.ActualStartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailOverlapCheckResult(1, OverlapStatus.OverlappingEndDate, isApproved));

            return this;
        }

        public CohortDomainServiceTestFixture WithNoActualStartDateEmailOverlaps()
        {
            DraftApprenticeshipDetails.Email = "test@test.com";
            DraftApprenticeshipDetails.ActualStartDate = new DateTime(2020, 1, 1);
            DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

            return this;
        }

        public void VerifyCheckForEmailOverlapsIsCalledCorrectlyWhenCreatingCohortWithInitialApprenticeship()
        {
            OverlapCheckService.Verify(x => x.CheckForEmailOverlaps(DraftApprenticeshipDetails.Email,
                It.Is<DateRange>(p =>
                    p.From == DraftApprenticeshipDetails.StartDate && p.To == DraftApprenticeshipDetails.EndDate),
                0, null, It.IsAny<CancellationToken>()));
        }

        public void VerifyCheckForActualStartDateEmailOverlapsIsCalledCorrectlyWhenCreatingCohortWithInitialApprenticeship()
        {
            OverlapCheckService.Verify(x => x.CheckForEmailOverlaps(DraftApprenticeshipDetails.Email,
                It.Is<DateRange>(p =>
                    p.From == DraftApprenticeshipDetails.ActualStartDate && p.To == DraftApprenticeshipDetails.EndDate),
                0, null, It.IsAny<CancellationToken>()));
        }

        public CohortDomainServiceTestFixture WithStartDate(DateTime? startDate)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            DraftApprenticeshipDetails.StartDate = utcStartDate;
            return this;
        }

        public CohortDomainServiceTestFixture WithActualStartDate(DateTime? startDate)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            DraftApprenticeshipDetails.ActualStartDate = utcStartDate;
            return this;
        }

        public CohortDomainServiceTestFixture WithEndDate(DateTime? endDate)
        {
            var utcEndDate = endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            DraftApprenticeshipDetails.EndDate = utcEndDate;
            return this;
        }

        public CohortDomainServiceTestFixture WithCohortMappedToProviderAndAccountLegalEntity(Party creatingParty, Party withParty = Party.None)
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                WithParty = withParty,
                Originator = creatingParty.ToOriginator(),
                EditStatus = (withParty == Party.Employer || withParty == Party.Provider) ? withParty.ToEditStatus() : EditStatus.Both,
                Provider = Provider.Object,
                ProviderId = ProviderId,
                EmployerAccountId = AccountId,
                AccountLegalEntityId = AccountLegalEntityId,
                AccountLegalEntity = AccountLegalEntity.Object,
                TransferSenderId = null,
            };

            var cohorts = new List<Cohort> { Cohort };

            Provider.Setup(x => x.Cohorts).Returns(cohorts);
            AccountLegalEntity.Setup(x => x.Cohorts).Returns(cohorts);

            Db.Cohorts.Add(Cohort);

            return this;
        }

        public CohortDomainServiceTestFixture WithChangeOfProviderCohort(Party withParty = Party.None)
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                WithParty = withParty,
                Originator = Originator.Employer,
                EditStatus = (withParty == Party.Employer || withParty == Party.Provider) ? withParty.ToEditStatus() : EditStatus.Both,
                Provider = Provider.Object,
                ProviderId = ProviderId,
                EmployerAccountId = AccountId,
                AccountLegalEntityId = AccountLegalEntityId,
                AccountLegalEntity = AccountLegalEntity.Object,
                TransferSenderId = null,
                ChangeOfPartyRequestId = ChangeOfPartyRequest.Object.Id
            };

            var cohorts = new List<Cohort> { Cohort };

            Provider.Setup(x => x.Cohorts).Returns(cohorts);
            AccountLegalEntity.Setup(x => x.Cohorts).Returns(cohorts);

            Db.Cohorts.Add(Cohort);

            ChangeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeProvider);

            return this;
        }

        public CohortDomainServiceTestFixture WithChangeOfEmployerCohort(Party withParty = Party.None)
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                WithParty = withParty,
                Originator = Originator.Provider,
                EditStatus = (withParty == Party.Employer || withParty == Party.Provider) ? withParty.ToEditStatus() : EditStatus.Both,
                Provider = Provider.Object,
                ProviderId = ProviderId,
                EmployerAccountId = AccountId,
                AccountLegalEntityId = AccountLegalEntityId,
                AccountLegalEntity = AccountLegalEntity.Object,
                TransferSenderId = null,
                ChangeOfPartyRequestId = ChangeOfPartyRequest.Object.Id
            };

            var cohorts = new List<Cohort> { Cohort };

            Provider.Setup(x => x.Cohorts).Returns(cohorts);
            AccountLegalEntity.Setup(x => x.Cohorts).Returns(cohorts);

            Db.Cohorts.Add(Cohort);

            ChangeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeEmployer);

            return this;
        }
        public CohortDomainServiceTestFixture WithExistingCohortApprovedByAllParties(Party creatingParty)
        {
            WithCohortMappedToProviderAndAccountLegalEntity(creatingParty);
            return this;
        }

        public CohortDomainServiceTestFixture WithExistingUnapprovedCohort()
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                EditStatus = EditStatus.Neither,
                TransferSenderId = null
            };

            Db.Cohorts.Add(Cohort);

            return this;
        }

        public CohortDomainServiceTestFixture WithExistingUnapprovedTransferCohort()
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                EditStatus = EditStatus.EmployerOnly,
                TransferSenderId = 11212,
                Approvals = Party.None,
                WithParty = Party.Employer
            };

            Db.Cohorts.Add(Cohort);

            return this;
        }

        public CohortDomainServiceTestFixture WithExistingDraftApprenticeship()
        {
            DraftApprenticeshipDetails.Id = DraftApprenticeshipId;
            Db.DraftApprenticeships.Add(ExistingDraftApprenticeship);
            return this;
        }

        public CohortDomainServiceTestFixture WithExistingDraftApprenticeshipWithPriorLearning()
        {
            DraftApprenticeshipDetails.Id = DraftApprenticeshipId;

            ExistingDraftApprenticeship.RecognisePriorLearning = true;
            ExistingDraftApprenticeship.PriorLearning = PriorLearning;

            Db.DraftApprenticeships.Add(ExistingDraftApprenticeship);
            return this;
        }


        public CohortDomainServiceTestFixture WithOverlappingEmails()
        {
            var fixture = new Fixture();
            var list = fixture.CreateMany<EmailOverlapCheckResult>().ToList();
            OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return this;
        }

        public void VerifyCheckForEmailOverlapsOnCohortIsCalledCorrectlyWhenApproving()
        {
            OverlapCheckService.Verify(x => x.CheckForEmailOverlaps(CohortId, It.IsAny<CancellationToken>()));
        }

        public CohortDomainServiceTestFixture WithContinuation(bool overlap)
        {
            long? changeOfPartyRequestId = ChangeOfPartyRequest.Object.Id;
            ExistingDraftApprenticeship.SetValue(x => x.ContinuationOfId, PreviousApprenticeship.Id);
            Cohort.SetValue(x => x.ChangeOfPartyRequestId, changeOfPartyRequestId);
            DraftApprenticeshipDetails.FirstName = ExistingDraftApprenticeship.FirstName;
            DraftApprenticeshipDetails.LastName = ExistingDraftApprenticeship.LastName;
            DraftApprenticeshipDetails.DateOfBirth = ExistingDraftApprenticeship.DateOfBirth;
            DraftApprenticeshipDetails.Uln = ExistingDraftApprenticeship.Uln;
            DraftApprenticeshipDetails.StartDate = ExistingDraftApprenticeship.StartDate;
            DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme(ExistingDraftApprenticeship.CourseCode, "", ProgrammeType.Framework, ReferenceDate, ReferenceDate);

            if (overlap)
            {
                PreviousApprenticeship.SetValue(x => x.StopDate, ExistingDraftApprenticeship.StartDate.Value.AddMonths(1));
            }
            else
            {
                PreviousApprenticeship.SetValue(x => x.StopDate, ExistingDraftApprenticeship.StartDate.Value.AddMonths(-1));
            }

            return this;
        }

        public CohortDomainServiceTestFixture WithParty(Party party)
        {
            Party = party;
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(Party);
            RequestingParty = party;
            return this;
        }

        public CohortDomainServiceTestFixture WithEmail(string email)
        {
            DraftApprenticeshipDetails.Email = email;
            return this;
        }

        public CohortDomainServiceTestFixture WithNoUserInfo()
        {
            UserInfo = null;
            return this;
        }

        public CohortDomainServiceTestFixture WithDecodeOfPublicHashedAccountLegalEntity()
        {
            EncodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(AccountLegalEntityId);
            return this;
        }

        public CohortDomainServiceTestFixture WithAgreementSignedAs(bool signed)
        {
            EmployerAgreementService.Setup(x => x.IsAgreementSigned(It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<AgreementFeature[]>())).ReturnsAsync(signed);
            return this;
        }

        public CohortDomainServiceTestFixture WithPledgeApplicationStatus(PledgeApplication.ApplicationStatus status)
        {
            PledgeApplication.Status = status;
            return this;
        }

        public async Task<Cohort> CreateCohort(long? accountId = null, long? accountLegalEntityId = null, long? transferSenderId = null, int? pledgeApplicationId = null, bool ignoreStartDateOverlap = false)
        {
            DraftApprenticeshipDetails.IgnoreStartDateOverlap = ignoreStartDateOverlap;
            await Db.SaveChangesAsync();
            DomainErrors.Clear();

            accountId ??= AccountId;
            accountLegalEntityId ??= AccountLegalEntityId;

            try
            {
                var result = await CohortDomainService.CreateCohort(ProviderId, accountId.Value, accountLegalEntityId.Value, transferSenderId, pledgeApplicationId,
                    DraftApprenticeshipDetails, UserInfo, RequestingParty, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart, CancellationToken.None);
                await Db.SaveChangesAsync();
                return result;
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
                if (Db.Cohorts.Contains(Cohort)) { Db.Cohorts.Remove(Cohort); }
                return null;
            }
            catch (Exception ex)
            {
                Exception = ex;
                if (Db.Cohorts.Contains(Cohort)) { Db.Cohorts.Remove(Cohort); }
                return null;
            }
        }

        public async Task CreateCohortWithOtherParty(long? transferSenderId = null, int? pledgeApplicationId = null)
        {
            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                var result = await CohortDomainService.CreateCohortWithOtherParty(ProviderId, AccountId, AccountLegalEntityId, transferSenderId, pledgeApplicationId, Message, UserInfo, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public async Task CreateEmptyCohort()
        {
            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                var result = await CohortDomainService.CreateEmptyCohort(ProviderId, AccountId, AccountLegalEntityId, UserInfo, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public async Task AddDraftApprenticeship(bool ignoreStartDateOverlap = false)
        {
            DraftApprenticeshipDetails.IgnoreStartDateOverlap = ignoreStartDateOverlap;

            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                await CohortDomainService.AddDraftApprenticeship(ProviderId, CohortId, DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart, RequestingParty, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
            }
        }

        public CohortDomainServiceTestFixture WithUlnOverlap(bool hasOverlap)
        {
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new OverlapCheckResult(hasOverlap, hasOverlap));

            return this;
        }

        public async Task ApproveCohort()
        {
            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                await CohortDomainService.ApproveCohort(CohortId, Message, UserInfo, RequestingParty, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                Exception = ex;
                DomainErrors.AddRange(ex.DomainErrors);
            }
        }

        public async Task SendCohortToOtherParty()
        {
            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                await CohortDomainService.SendCohortToOtherParty(CohortId, Message, UserInfo, RequestingParty, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
            }
        }

        public async Task UpdateDraftApprenticeship(bool ignoreStartDateOverlap = false)
        {
            DraftApprenticeshipDetails.IgnoreStartDateOverlap = ignoreStartDateOverlap;
            if (Party == Party.Employer)
            {
                DraftApprenticeshipDetails.Uln = ExistingDraftApprenticeship.Uln;
            }

            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                await CohortDomainService.UpdateDraftApprenticeship(CohortId, DraftApprenticeshipDetails, UserInfo, RequestingParty, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                Exception = ex;
                DomainErrors.AddRange(ex.DomainErrors);
            }
        }

        public async Task DeleteDraftApprenticeship()
        {
            Db.SaveChanges();
            DomainErrors.Clear();

            try
            {
                await CohortDomainService.DeleteDraftApprenticeship(CohortId, DraftApprenticeshipId, UserInfo, CancellationToken.None);
                await Db.SaveChangesAsync();
            }
            catch (DomainException ex)
            {
                DomainErrors.AddRange(ex.DomainErrors);
            }
        }

        public void VerifyCohortCreation(Party party)
        {
            if (party == Party.Provider)
            {
                Provider.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), null, null,
                    DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart));
            }

            if (party == Party.Employer)
            {
                AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), null, null,
                    DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart));
            }
        }

        public void VerifyEmptyCohortCreation(Party party)
        {
            if (party == Party.Provider)
            {
                Provider.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), UserInfo));
            }
        }

        public void VerifyCohortCreationWithTransferSender(Party party, int? pledgeApplicationId)
        {
            if (party == Party.Provider)
            {
                Provider.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), UserInfo));
            }

            if (party == Party.Employer)
            {
                AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(t => t.Id == TransferSenderId && t.Name == TransferSenderName), It.Is<int?>(p => p == pledgeApplicationId),
                    DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart));
            }
        }

        public void VerifyCohortCreationWithoutTransferSender(Party party)
        {
            if (party == Party.Provider)
            {
                Provider.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(p => p == null), It.Is<int?>(p => p == null),
                    DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart));
            }

            if (party == Party.Employer)
            {
                AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(p => p == null), It.Is<int?>(p => p == null),
                    DraftApprenticeshipDetails, UserInfo, Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart));
            }
        }

        public void VerifyCohortCreationWithOtherParty_WithoutTransferSender()
        {
            AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), It.Is<Account>(t => t == null), It.Is<int?>(p => p == null), Message, UserInfo));
        }

        public void VerifyCohortCreationWithOtherParty_WithTransferSender()
        {
            AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(ProviderId,
                It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object),
                It.Is<Account>(t => t.Id == TransferSenderId && t.Name == TransferSenderName),
                It.IsAny<int?>(),
                Message,
                UserInfo));
        }

        public void VerifyCohortCreationWithOtherParty_WithPledgeApplicationId()
        {
            AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(ProviderId,
                It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object),
                It.Is<Account>(t => t.Id == TransferSenderId && t.Name == TransferSenderName),
                It.Is<int?>(p => p == PledgeApplicationId),
                Message,
                UserInfo));
        }

        public void VerifyProviderDraftApprenticeshipAdded()
        {
            Assert.That(Cohort.DraftApprenticeships.Any(), Is.True);
        }

        public void VerifyDraftApprenticeshipUpdated()
        {
            var updated = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);

            Assert.Multiple(() =>
            {
                Assert.That(updated, Is.Not.Null, "No draft apprenticeship record found");
                Assert.That(DraftApprenticeshipDetails.FirstName, Is.EqualTo(updated.FirstName));
                Assert.That(DraftApprenticeshipDetails.LastName, Is.EqualTo(updated.LastName));
            });
        }

        public void VerifyPriorLearningIsNull()
        {
            var updated = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);

            Assert.Multiple(() =>
            {
                Assert.That(updated.PriorLearning, Is.Not.Null, "No prior learning found");
                Assert.That(updated.TrainingTotalHours, Is.Null);
                Assert.That(updated.PriorLearning.DurationReducedByHours, Is.Null);
                Assert.That(updated.PriorLearning.IsDurationReducedByRpl, Is.Null);
                Assert.That(updated.PriorLearning.DurationReducedBy, Is.Null);
                Assert.That(updated.PriorLearning.PriceReducedBy, Is.Null);
            });
        }

        public void VerifyPriorLearningIsStillPresent()
        {
            var updated = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);

            Assert.Multiple(() =>
            {
                Assert.That(updated.PriorLearning, Is.Not.Null, "No prior learning found");

                Assert.That(updated.PriorLearning.DurationReducedByHours, Is.Not.Null);
                Assert.That(updated.PriorLearning.DurationReducedByHours, Is.EqualTo(PriorLearning.DurationReducedByHours));
                Assert.That(updated.PriorLearning.IsDurationReducedByRpl, Is.Not.Null);

                Assert.That(updated.PriorLearning.IsDurationReducedByRpl, Is.EqualTo(PriorLearning.IsDurationReducedByRpl));
                Assert.That(updated.PriorLearning.DurationReducedBy, Is.Not.Null);

                Assert.That(updated.PriorLearning.DurationReducedBy, Is.EqualTo(PriorLearning.DurationReducedBy));
                Assert.That(updated.PriorLearning.PriceReducedBy, Is.Not.Null);

                Assert.That(updated.PriorLearning.PriceReducedBy, Is.EqualTo(PriorLearning.PriceReducedBy));
            });
        }

        public void VerifyPriorLearningIsNotSetToNewRPLValues()
        {
            var updated = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);
            Assert.That(updated.RecognisePriorLearning != DraftApprenticeshipDetails.RecognisePriorLearning &&
                        updated.TrainingTotalHours != DraftApprenticeshipDetails.TrainingTotalHours &&
                        updated.PriorLearning?.DurationReducedByHours != DraftApprenticeshipDetails.DurationReducedByHours &&
                        updated.PriorLearning?.IsDurationReducedByRpl != DraftApprenticeshipDetails.IsDurationReducedByRPL &&
                        updated.PriorLearning?.DurationReducedBy != DraftApprenticeshipDetails.DurationReducedBy &&
                        updated.PriorLearning?.PriceReducedBy != DraftApprenticeshipDetails.PriceReducedBy, Is.False);
        }

        public void VerifyPriorLearningIsSetToNewRPLValues()
        {
            var updated = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);
            Assert.That(updated.RecognisePriorLearning == DraftApprenticeshipDetails.RecognisePriorLearning &&
                        updated.TrainingTotalHours == DraftApprenticeshipDetails.TrainingTotalHours &&
                        updated.PriorLearning?.DurationReducedByHours == DraftApprenticeshipDetails.DurationReducedByHours &&
                        updated.PriorLearning?.IsDurationReducedByRpl == DraftApprenticeshipDetails.IsDurationReducedByRPL &&
                        updated.PriorLearning?.DurationReducedBy == DraftApprenticeshipDetails.DurationReducedBy &&
                        updated.PriorLearning?.PriceReducedBy == DraftApprenticeshipDetails.PriceReducedBy, Is.False);
        }

        public void VerifyLastUpdatedFieldsAreSet(Party withParty)
        {
            switch (withParty)
            {
                case Party.Employer:
                    Assert.That(UserInfo.UserDisplayName, Is.EqualTo(Cohort.LastUpdatedByEmployerName));
                    Assert.That(UserInfo.UserEmail, Is.EqualTo(Cohort.LastUpdatedByEmployerEmail));
                    break;
                case Party.Provider:
                    Assert.That(UserInfo.UserDisplayName, Is.EqualTo(Cohort.LastUpdatedByProviderName));
                    Assert.That(UserInfo.UserEmail, Is.EqualTo(Cohort.LastUpdatedByProviderEmail));
                    break;
                default:
                    Assert.Fail("Party must be provider or Employer");
                    break;
            }
        }

        public void VerifyLastUpdatedFieldsAreNotSet()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Cohort.LastUpdatedByEmployerName, Is.Null);
                Assert.That(Cohort.LastUpdatedByEmployerEmail, Is.Null);
                Assert.That(Cohort.LastUpdatedByProviderName, Is.Null);
                Assert.That(Cohort.LastUpdatedByProviderEmail, Is.Null);
            });
        }
        public void VerifyStartDateException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "StartDate"), Is.True);
        }

        public void VerifyActualStartDateException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "ActualStartDate"), Is.True);
        }

        public void VerifyEndDateException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "EndDate"), Is.True);
        }

        public void VerifyUlnException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "Uln"), Is.True);
        }

        public void VerifyEmailException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "Email"), Is.True);
        }

        public void VerifyReservationException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "TEST"), Is.True);
        }

        public void VerifyCourseException(bool passes)
        {
            if (passes)
            {
                Assert.That(DomainErrors.Any(), Is.False);
                return;
            }

            Assert.That(DomainErrors.Any(x => x.PropertyName == "CourseCode"), Is.True);
        }

        public void VerifyReservationValidationNotPerformed()
        {
            ReservationValidationService.Verify(x => x.Validate(It.IsAny<ReservationValidationRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        public void VerifyOverlapExceptionOnStartDate(string otherParty)
        {
            Assert.That(DomainErrors.Any(x => x.PropertyName == "StartDate" && x.ErrorMessage.Contains($"contact the {otherParty}")), Is.True);
        }

        public void VerifyOverlapExceptionOnActualStartDate(string otherParty)
        {
            Assert.That(DomainErrors.Any(x => x.PropertyName == "ActualStartDate" && x.ErrorMessage.Contains($"contact the {otherParty}")), Is.True);
        }

        public void VerifyOverlapExceptionOnEndDate(string otherParty)
        {
            Assert.That(DomainErrors.Any(x => x.PropertyName == "EndDate" && x.ErrorMessage.Contains($"contact the {otherParty}")), Is.True);
        }

        public void VerifyEmailOverlapExceptionOnApprenticeship(bool isApproved)
        {
            var expectedErrorMessage = isApproved
                ? "This email address is in use on another apprentice record. You need to enter a different email address."
                : "You need to enter a unique email address for each apprentice.";
            Assert.That(DomainErrors.Any(x => x.PropertyName == "Email" && x.ErrorMessage == expectedErrorMessage), Is.True);
        }

        public void VerifyNoUlnOverlaps()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DomainErrors.Any(x => x.PropertyName == "StartDate"), Is.False);
                Assert.That(DomainErrors.Any(x => x.PropertyName == "EndDate"), Is.False);
            });
        }

        public void VerifyException<T>()
        {
            Assert.That(Exception, Is.Not.Null);
            Assert.That(Exception, Is.InstanceOf<T>());
        }

        public void VerifyNoException()
        {
            Assert.That(Exception, Is.Null);
        }

        public void VerifyIsAgreementSignedIsCalledCorrectly()
        {
            EmployerAgreementService.Verify(x => x.IsAgreementSigned(AccountId, MaLegalEntityId,
                It.IsAny<AgreementFeature[]>()));
        }

        public void VerifyDraftApprenticeshipDeleted()
        {
            var deleted = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);

            Assert.That(deleted, Is.Null, "Draft apprenticeship record not deleted");
        }

        public void TearDown()
        {
            Db.Database.EnsureDeleted();
        }
    }
}