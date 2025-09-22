using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.CreateCohort;

[TestFixture]
public class WhenCohortIsCreated
{
    private WhenCohortIsCreatedTestFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new WhenCohortIsCreatedTestFixture();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_It_Belongs_To_The_Correct_Provider(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyProvider();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_It_Belongs_To_The_Correct_EmployerAccount(ChangeOfPartyRequestType requestType)
    {
        //EmployerAccount could be the one on the apprenticeship, or the one on the COPR itself, depending on the request type
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyAccountId();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_It_Belongs_To_The_Correct_EmployerAccountLegalEntity(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyAccountLegalEntityId();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_The_Originator_Is_Correct(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyOriginator();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, true)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, true)]

    public void Then_The_DraftApprenticeshipDetails_Are_Correct(ChangeOfPartyRequestType requestType, bool isContinuation)
    {
        _fixture.WithChangeOfPartyType(requestType);
        if (isContinuation) _fixture.WithContinuation();
        _fixture.CreateCohort();
        _fixture.VerifyDraftApprenticeshipDetails();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, true)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, true)]

    public void Then_The_DraftApprenticeshipDetails_FlexibleEmployment_Are_Correct(ChangeOfPartyRequestType requestType, bool isContinuation)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.WithFlexibleApprenticeship();
        if (isContinuation) _fixture.WithContinuation();
        _fixture.CreateCohort();
        _fixture.VerifyDraftApprenticeshipDetailsFlexibleEmployment();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, true)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, true)]
    public void Then_The_DraftApprenticeshipDetails_ApprenticeshipConfirmationStats_Are_Correct(ChangeOfPartyRequestType requestType, bool isContinuation)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.WithApprenticeshipConfirmedStatus();
        if (isContinuation) _fixture.WithContinuation();
        _fixture.CreateCohort();
        _fixture.VerifyApprenticeshipConfirmationDetails();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_WithParty_Is_Correct(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyWithOtherParty();
    }

    [Test]
    public void By_The_Provider_Then_Originator_Approval_Is_Given()
    {
        _fixture.WithChangeOfPartyType(ChangeOfPartyRequestType.ChangeEmployer);
        _fixture.CreateCohort();
        _fixture.VerifyProviderOriginatorApproval();
    }
        
    [Test]
    public void Change_Of_Employer_With_Overlap_Then_Originator_Approval_Is_Not_Given()
    {
        _fixture
            .WithChangeOfPartyType(ChangeOfPartyRequestType.ChangeEmployer)
            .WithOverlappingTrainingDates();
        _fixture.CreateCohort();
        _fixture.VerifyNoApproval();
    }
        
    [Test]
    public void Change_Of_Employer_With_Overlap_Then_Cohort_Remains_With_Provider()
    {
        _fixture
            .WithChangeOfPartyType(ChangeOfPartyRequestType.ChangeEmployer)
            .WithOverlappingTrainingDates();
        _fixture.CreateCohort();
        _fixture.VerifyWithSameParty();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void By_The_Employer_And_They_Have_Supplied_All_Details_Then_Originator_Approval_Is_Given(bool isEmployerLed)
    {
        _fixture.WithChangeOfPartyType(ChangeOfPartyRequestType.ChangeProvider, isEmployerLed);
        _fixture.CreateCohort();
        _fixture.VerifyEmployerOriginatorApproval(isEmployerLed);
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_It_Is_Not_A_Draft(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyCohortIsNotDraft();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_ChangeOfPartyCohortCreatedEvent_Is_Emitted(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyCohortWithChangeOfPartyCreatedEvent();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_Cohort_Creation_Is_Subject_To_StateTracking(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyTracking();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer, false)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider, true)]
    public void Then_TransferSenderId_Is_Correct(ChangeOfPartyRequestType requestType, bool expectTransferSenderId)
    {
        _fixture
            .WithChangeOfPartyType(requestType)
            .WithTransferSender();
        _fixture.CreateCohort();
        _fixture.VerifyTransferSender();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_ChangeOfPartyRequestId_Is_Correct(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyChangeOfPartyRequestId();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void Then_AssignedToOtherPartyEvent_Is_Emitted(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();
        _fixture.VerifyAssignedToOtherPartyEventIsEmitted();
    }

    [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
    [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
    public void TheDraftApprenticeshipCreatedEventIsPublished(ChangeOfPartyRequestType requestType)
    {
        _fixture.WithChangeOfPartyType(requestType);
        _fixture.CreateCohort();

        _fixture.VerifyDraftApprenticeshipCreatedEventIsPublished();
    }

    private class WhenCohortIsCreatedTestFixture
    {
        private Fixture _autoFixture = new Fixture();
        public CommitmentsV2.Models.Apprenticeship ContinuedApprenticeship { get; private set; }
        public CommitmentsV2.Models.ChangeOfPartyRequest Request { get; private set; }
        public Guid ReservationId { get; set; }
        public UserInfo UserInfo { get; set; }
        public CommitmentsV2.Models.Cohort Result { get; private set; }
        public Exception Exception { get; private set; }
        public UnitOfWorkContext UnitOfWorkContext { get; private set; }
        public bool HasOverlappingTrainingDates { get; private set; }

        public WhenCohortIsCreatedTestFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();

            ReservationId = _autoFixture.Create<Guid>();
            UserInfo = _autoFixture.Create<UserInfo>();
            HasOverlappingTrainingDates = false;

            var cohort = new CommitmentsV2.Models.Cohort();
            cohort.SetValue(x => x.ProviderId, _autoFixture.Create<long>());

            ContinuedApprenticeship = new CommitmentsV2.Models.Apprenticeship();
            ContinuedApprenticeship.SetValue(x => x.Id, _autoFixture.Create<long>());
            ContinuedApprenticeship.SetValue(x => x.Cohort, cohort);
            ContinuedApprenticeship.SetValue(x => x.CommitmentId, cohort.Id);
            ContinuedApprenticeship.SetValue(x => x.FirstName, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.LastName, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.DateOfBirth, _autoFixture.Create<DateTime>());
            ContinuedApprenticeship.SetValue(x => x.Uln, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.StartDate, _autoFixture.Create<DateTime?>());
            ContinuedApprenticeship.SetValue(x => x.EndDate, _autoFixture.Create<DateTime?>());
            ContinuedApprenticeship.SetValue(x => x.StartDate, _autoFixture.Create<DateTime?>());
            ContinuedApprenticeship.SetValue(x => x.DeliveryModel, _autoFixture.Create<DeliveryModel>());
            ContinuedApprenticeship.SetValue(x => x.CourseCode, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.CourseName, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.ProgrammeType, _autoFixture.Create<ProgrammeType>());
            ContinuedApprenticeship.SetValue(x => x.EmployerRef, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.ProviderRef, _autoFixture.Create<string>());
            ContinuedApprenticeship.SetValue(x => x.RecognisePriorLearning, true);
            ContinuedApprenticeship.SetValue(x => x.PriorLearning, _autoFixture.Create<ApprenticeshipPriorLearning>());

            Request = new CommitmentsV2.Models.ChangeOfPartyRequest();
            Request.SetValue(x => x.Apprenticeship, ContinuedApprenticeship);
            Request.SetValue(x => x.ApprenticeshipId, ContinuedApprenticeship.Id);
            Request.SetValue(x => x.StartDate, _autoFixture.Create<DateTime?>());
            Request.SetValue(x => x.Price, _autoFixture.Create<int?>());
            Request.SetValue(x => x.OriginatingParty, _autoFixture.Create<Party>());
        }

        public WhenCohortIsCreatedTestFixture WithChangeOfPartyType(ChangeOfPartyRequestType value, bool employerLed = false)
        {
            Request.SetValue(x => x.ChangeOfPartyType, value);
            Request.SetValue(x => x.OriginatingParty, value == ChangeOfPartyRequestType.ChangeEmployer ? Party.Provider : Party.Employer);

            if (value == ChangeOfPartyRequestType.ChangeEmployer)
            {
                var accountLegalEntity = new AccountLegalEntity();
                accountLegalEntity.SetValue(x => x.Id, _autoFixture.Create<long>());
                accountLegalEntity.SetValue(x => x.Account, new Account());
                accountLegalEntity.Account.SetValue(x => x.Id, _autoFixture.Create<long>());
                accountLegalEntity.SetValue(x => x.AccountId, accountLegalEntity.Account.Id);
                Request.SetValue(x => x.AccountLegalEntity, accountLegalEntity);
                Request.SetValue(x => x.AccountLegalEntityId, accountLegalEntity?.Id);
                ContinuedApprenticeship.Cohort.SetValue(x => x.ProviderId, _autoFixture.Create<long>());
            }
            else
            {
                if (employerLed)
                {
                    var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    Request.SetValue(x => x.StartDate, date);
                    Request.SetValue(x => x.EndDate, date.AddYears(1));
                    Request.SetValue(x => x.Price, 1000);
                }

                Request.SetValue(x => x.ProviderId, _autoFixture.Create<long>());
                ContinuedApprenticeship.Cohort.SetValue(x => x.AccountLegalEntityId, _autoFixture.Create<long>());
                ContinuedApprenticeship.Cohort.SetValue(x => x.EmployerAccountId, _autoFixture.Create<long>());
            }

            return this;
        }

        internal void WithFlexibleApprenticeship()
        {
            ContinuedApprenticeship.DeliveryModel = DeliveryModel.PortableFlexiJob;
            Request.SetValue(x => x.EmploymentEndDate, _autoFixture.Create<DateTime>());
            Request.SetValue(x => x.EmploymentPrice, _autoFixture.Create<int>());
        }

        internal void WithApprenticeshipConfirmedStatus()
        {
            ContinuedApprenticeship.SetValue(x => x.ApprenticeshipConfirmationStatus, _autoFixture.Build<ApprenticeshipConfirmationStatus>().Without(x => x.Apprenticeship).Create());
            ContinuedApprenticeship.SetValue(x => x.EmailAddressConfirmed, true);
        }

        public WhenCohortIsCreatedTestFixture WithContinuation()
        {
            ContinuedApprenticeship.ContinuationOfId = _autoFixture.Create<long>();
            ContinuedApprenticeship.OriginalStartDate = _autoFixture.Create<DateTime>();
            return this;
        }

        public WhenCohortIsCreatedTestFixture WithTransferSender()
        {
            Request.Apprenticeship.Cohort.TransferSenderId = _autoFixture.Create<long>();
            return this;
        }

        public void CreateCohort()
        {
            try
            {
                Result = Request.CreateCohort(ContinuedApprenticeship, ReservationId, UserInfo, HasOverlappingTrainingDates);
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        public void VerifyProvider()
        {
            Assert.That(Result.ProviderId, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? ContinuedApprenticeship.Cohort.ProviderId : Request.ProviderId));
        }

        public void VerifyAccountId()
        {
            Assert.That(Result.EmployerAccountId, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? Request.AccountLegalEntity?.AccountId : ContinuedApprenticeship.Cohort.EmployerAccountId));
        }

        public void VerifyAccountLegalEntityId()
        {
            Assert.That(Result.AccountLegalEntityId, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? Request.AccountLegalEntityId.Value : ContinuedApprenticeship.Cohort.AccountLegalEntityId));
        }

        public void VerifyOriginator()
        {
            Assert.That(Result.Originator.ToParty(), Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? Party.Provider : Party.Employer));
        }

        public void VerifyWithOtherParty()
        {
            Assert.That(Result.WithParty, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? Party.Employer : Party.Provider));
        }
            
        public void VerifyWithSameParty()
        {
            Assert.That(
                Result.WithParty, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                    ? Party.Provider
                    : Party.Employer));
        }

        public void VerifyProviderOriginatorApproval()
        {
            Assert.That(Result.Approvals, Is.EqualTo(Party.Provider));
        }

        public void VerifyEmployerOriginatorApproval(bool isEmployerLed)
        {
            Assert.That(Result.Approvals, Is.EqualTo(isEmployerLed ? Party.Employer : Party.None));
        }

        public void VerifyCohortIsNotDraft()
        {
            Assert.That(Result.IsDraft, Is.False);
        }

        public void VerifyDraftApprenticeshipDetails()
        {
                
            var draftApprenticeship = Result.DraftApprenticeships.Single();
            Assert.Multiple(() =>
            {
                Assert.That(Result.DraftApprenticeships.Count(), Is.EqualTo(1));
                Assert.That(draftApprenticeship.FirstName, Is.EqualTo(ContinuedApprenticeship.FirstName));
                Assert.That(draftApprenticeship.LastName, Is.EqualTo(ContinuedApprenticeship.LastName));
                Assert.That(draftApprenticeship.Email, Is.EqualTo(ContinuedApprenticeship.Email));
                Assert.That(draftApprenticeship.DateOfBirth, Is.EqualTo(ContinuedApprenticeship.DateOfBirth));
                Assert.That(draftApprenticeship.Uln, Is.EqualTo(ContinuedApprenticeship.Uln));
                Assert.That(draftApprenticeship.StartDate, Is.EqualTo(Request.StartDate));
                Assert.That(draftApprenticeship.EndDate, Is.EqualTo(Request.EndDate));
                Assert.That(draftApprenticeship.DeliveryModel, Is.EqualTo(ContinuedApprenticeship.DeliveryModel));
                Assert.That(draftApprenticeship.CourseCode, Is.EqualTo(ContinuedApprenticeship.CourseCode));
                Assert.That(draftApprenticeship.CourseName, Is.EqualTo(ContinuedApprenticeship.CourseName));
                Assert.That(draftApprenticeship.ProgrammeType, Is.EqualTo(ContinuedApprenticeship.ProgrammeType));
                Assert.That(draftApprenticeship.Cost, Is.EqualTo(Request.Price));
                Assert.That(draftApprenticeship.EmployerRef, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? string.Empty : ContinuedApprenticeship.EmployerRef));
                Assert.That(draftApprenticeship.ProviderRef, Is.EqualTo(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider ? string.Empty : ContinuedApprenticeship.ProviderRef));
                Assert.That(draftApprenticeship.ReservationId, Is.EqualTo(ReservationId));
                Assert.That(draftApprenticeship.ContinuationOfId, Is.EqualTo(ContinuedApprenticeship.Id));
                Assert.That(draftApprenticeship.OriginalStartDate, Is.EqualTo(ContinuedApprenticeship.OriginalStartDate ?? ContinuedApprenticeship.StartDate));
                Assert.That(draftApprenticeship.EmployerHasEditedCost, Is.EqualTo(ContinuedApprenticeship.EmployerHasEditedCost));
            });
        }

        public void VerifyDraftApprenticeshipDetailsFlexibleEmployment()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.DraftApprenticeships.Count(), Is.EqualTo(1));
                    
                var draftApprenticeship = Result.DraftApprenticeships.Single();
                Assert.That(draftApprenticeship.FlexibleEmployment, Is.Not.Null);
                Assert.That(draftApprenticeship.FlexibleEmployment.EmploymentEndDate, Is.EqualTo(Request.EmploymentEndDate));
                Assert.That(draftApprenticeship.FlexibleEmployment.EmploymentPrice, Is.EqualTo(Request.EmploymentPrice));
            });
        }

        public void VerifyApprenticeshipConfirmationDetails()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.DraftApprenticeships.Count(), Is.EqualTo(1));
                   
                var draftApprenticeship = Result.DraftApprenticeships.Single();
                Assert.That(draftApprenticeship.ApprenticeshipConfirmationStatus, Is.Not.Null);
                Assert.That(draftApprenticeship.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn, Is.EqualTo(ContinuedApprenticeship.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn));
                Assert.That(draftApprenticeship.ApprenticeshipConfirmationStatus.CommitmentsApprovedOn, Is.EqualTo(ContinuedApprenticeship.ApprenticeshipConfirmationStatus.CommitmentsApprovedOn));
                Assert.That(draftApprenticeship.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn, Is.EqualTo(ContinuedApprenticeship.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn));
                Assert.That(draftApprenticeship.EmailAddressConfirmed, Is.EqualTo(ContinuedApprenticeship.EmailAddressConfirmed));
            });
        }

        public void VerifyTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                           && @event.EntityType ==
                                                                           nameof(Cohort)), Is.Not.Null);
        }

        public void VerifyCohortWithChangeOfPartyCreatedEvent()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortWithChangeOfPartyCreatedEvent @event
                                                                           && @event.ChangeOfPartyRequestId == Request.Id
                                                                           && @event.CohortId == Result.Id
                                                                           && @event.OriginatingParty == Request.OriginatingParty
            ), Is.Not.Null);
        }

        public void VerifyTransferSender()
        {
            if (Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
            {
                Assert.That(Result.TransferSenderId, Is.Null);
            }
            else
            {
                Assert.That(Result.TransferSenderId, Is.EqualTo(ContinuedApprenticeship.Cohort.TransferSenderId));
            }
        }

        public void VerifyChangeOfPartyRequestId()
        {
            Assert.That(Result.ChangeOfPartyRequestId, Is.EqualTo(Request.Id));
        }

        public void VerifyAssignedToOtherPartyEventIsEmitted()
        {
            if (Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
            {
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x =>
                    x is CohortAssignedToEmployerEvent @event
                    && @event.AssignedBy == Party.Provider
                    && @event.CohortId == Result.Id), Is.Not.Null);
            }
            else
            {
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x =>
                    x is CohortAssignedToProviderEvent @event
                    && @event.CohortId == Result.Id), Is.Not.Null);
            }
        }
        public void VerifyDraftApprenticeshipCreatedEventIsPublished()
        {
            var draftApprenticeship = Result.Apprenticeships.Single();

            UnitOfWorkContext.GetEvents().OfType<DraftApprenticeshipCreatedEvent>().Should().ContainSingle(e =>
                e.CohortId == Result.Id &&
                e.DraftApprenticeshipId == draftApprenticeship.Id &&
                e.Uln == draftApprenticeship.Uln &&
                e.ReservationId == draftApprenticeship.ReservationId &&
                e.CreatedOn == draftApprenticeship.CreatedOn);
        }

        public void WithOverlappingTrainingDates()
        {
            HasOverlappingTrainingDates = true;
        }

        public void VerifyNoApproval()
        {
            Result.Approvals.Should().Be(Party.None);
        }
    }
}