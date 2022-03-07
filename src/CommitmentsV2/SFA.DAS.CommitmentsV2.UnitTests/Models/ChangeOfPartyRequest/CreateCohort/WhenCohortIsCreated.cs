using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.CreateCohort
{
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

            public WhenCohortIsCreatedTestFixture()
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                ReservationId = _autoFixture.Create<Guid>();
                UserInfo = _autoFixture.Create<UserInfo>();

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
                    Result = Request.CreateCohort(ContinuedApprenticeship, ReservationId, UserInfo);
                }
                catch (Exception e)
                {
                    Exception = e;
                }
            }

            public void VerifyProvider()
            {
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                                ? ContinuedApprenticeship.Cohort.ProviderId
                                : Request.ProviderId, 
                        Result.ProviderId);
            }

            public void VerifyAccountId()
            {
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Request.AccountLegalEntity?.AccountId
                        : ContinuedApprenticeship.Cohort.EmployerAccountId,
                    Result.EmployerAccountId);
            }

            public void VerifyAccountLegalEntityId()
            {
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Request.AccountLegalEntityId.Value
                        : ContinuedApprenticeship.Cohort.AccountLegalEntityId,
                    Result.AccountLegalEntityId);
            }

            public void VerifyOriginator()
            {
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Party.Provider
                        : Party.Employer,
                    Result.Originator.ToParty());
            }

            public void VerifyWithOtherParty()
            {
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Party.Employer
                        : Party.Provider,
                    Result.WithParty);
            }

            public void VerifyProviderOriginatorApproval()
            {
                Assert.AreEqual(Party.Provider, Result.Approvals);
            }

            public void VerifyEmployerOriginatorApproval(bool isEmployerLed)
            {
                Assert.AreEqual(isEmployerLed ? Party.Employer : Party.None, Result.Approvals);
            }

            public void VerifyCohortIsNotDraft()
            {
                Assert.IsFalse(Result.IsDraft);
            }

            public void VerifyDraftApprenticeshipDetails()
            {
                Assert.AreEqual(1, Result.DraftApprenticeships.Count());
                var draftApprenticeship = Result.DraftApprenticeships.Single();
                Assert.AreEqual(ContinuedApprenticeship.FirstName, draftApprenticeship.FirstName);
                Assert.AreEqual(ContinuedApprenticeship.LastName, draftApprenticeship.LastName);
                Assert.AreEqual(ContinuedApprenticeship.Email, draftApprenticeship.Email);
                Assert.AreEqual(ContinuedApprenticeship.DateOfBirth, draftApprenticeship.DateOfBirth);
                Assert.AreEqual(ContinuedApprenticeship.Uln, draftApprenticeship.Uln);
                Assert.AreEqual(Request.StartDate, draftApprenticeship.StartDate);
                Assert.AreEqual(Request.EndDate, draftApprenticeship.EndDate);
                Assert.AreEqual(ContinuedApprenticeship.DeliveryModel, draftApprenticeship.DeliveryModel);
                Assert.AreEqual(ContinuedApprenticeship.CourseCode, draftApprenticeship.CourseCode);
                Assert.AreEqual(ContinuedApprenticeship.CourseName, draftApprenticeship.CourseName);
                Assert.AreEqual(ContinuedApprenticeship.ProgrammeType, draftApprenticeship.ProgrammeType);
                Assert.AreEqual(Request.Price, draftApprenticeship.Cost);
                Assert.AreEqual(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? string.Empty : ContinuedApprenticeship.EmployerRef, draftApprenticeship.EmployerRef);
                Assert.AreEqual(Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider ? string.Empty : ContinuedApprenticeship.ProviderRef, draftApprenticeship.ProviderRef);
                Assert.AreEqual(ReservationId, draftApprenticeship.ReservationId);
                Assert.AreEqual(ContinuedApprenticeship.Id, draftApprenticeship.ContinuationOfId);
                Assert.AreEqual(ContinuedApprenticeship.OriginalStartDate ?? ContinuedApprenticeship.StartDate, draftApprenticeship.OriginalStartDate);
            }

            public void VerifyTracking()
            {
                Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(Cohort)));
            }

            public void VerifyCohortWithChangeOfPartyCreatedEvent()
            {
                Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortWithChangeOfPartyCreatedEvent @event
                                                                                    && @event.ChangeOfPartyRequestId == Request.Id
                                                                                    && @event.CohortId == Result.Id
                                                                                    && @event.OriginatingParty == Request.OriginatingParty
                                                                                    ));
            }

            public void VerifyTransferSender()
            {
                if (Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
                {
                    Assert.IsNull(Result.TransferSenderId);
                }
                else
                {
                    Assert.AreEqual(ContinuedApprenticeship.Cohort.TransferSenderId, Result.TransferSenderId);
                }
            }

            public void VerifyChangeOfPartyRequestId()
            {
                Assert.AreEqual(Request.Id, Result.ChangeOfPartyRequestId);
            }

            public void VerifyAssignedToOtherPartyEventIsEmitted()
            {
                if (Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
                {
                    Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x =>
                    x is CohortAssignedToEmployerEvent @event
                    && @event.AssignedBy == Party.Provider
                    && @event.CohortId == Result.Id));
                }
                else
                {
                    Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x =>
                        x is CohortAssignedToProviderEvent @event
                        && @event.CohortId == Result.Id));
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

        }
    }
}
