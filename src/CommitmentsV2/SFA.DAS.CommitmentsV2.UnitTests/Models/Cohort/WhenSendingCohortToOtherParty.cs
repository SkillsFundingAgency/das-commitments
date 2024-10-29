using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    [Parallelizable]
    public class WhenSendingCohortToOtherParty
    {
        private WhenSendingCohortToOtherPartyTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenSendingCohortToOtherPartyTestsFixture();
        }

        [TestCase(Party.Employer, EditStatus.ProviderOnly)]
        [TestCase(Party.Provider, EditStatus.EmployerOnly)]
        public void ThenShouldUpdateStatus(Party modifyingParty, EditStatus expectedEditStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SendToOtherParty();

            _fixture.Cohort.EditStatus.Should().Be(expectedEditStatus);
            _fixture.Cohort.LastAction.Should().Be(LastAction.Amend);
            _fixture.Cohort.CommitmentStatus.Should().Be(CommitmentStatus.Active);
        }

        [TestCase(Party.Employer, null, "", 0)]
        [TestCase(Party.Employer, "Hello Provider", "Hello Provider", 0)]
        [TestCase(Party.Provider, null, "", 1)]
        [TestCase(Party.Provider, "Hello Employer", "Hello Employer", 1)]
        public void ThenShouldAddMessage(Party modifyingParty, string message, string expectedMessage, byte expectedCreatedBy)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetMessage(message)
                .SendToOtherParty();

            _fixture.Cohort.Messages.Should().HaveCount(1)
                .And.ContainSingle(m =>
                    m.CreatedBy == expectedCreatedBy &&
                    m.Text == expectedMessage &&
                    m.Author == _fixture.UserInfo.UserDisplayName);
        }

        [TestCase(Party.Employer, "Employer", "foo@foo.com", "Employer", "foo@foo.com", null, null)]
        [TestCase(Party.Provider, "Provider", "bar@bar.com", null, null, "Provider", "bar@bar.com")]
        public void ThenShouldSetLastUpdatedBy(Party modifyingParty, string userDisplayName, string userEmail, string expectedLastUpdatedByEmployerName, string expectedLastUpdatedByEmployerEmail, string expectedLastUpdatedByProviderName, string expectedLastUpdatedByProviderEmail)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetUserInfo(userDisplayName, userEmail)
                .SendToOtherParty();

            _fixture.Cohort.LastUpdatedByEmployerName.Should().Be(expectedLastUpdatedByEmployerName);
            _fixture.Cohort.LastUpdatedByEmployerEmail.Should().Be(expectedLastUpdatedByEmployerEmail);
            _fixture.Cohort.LastUpdatedByProviderName.Should().Be(expectedLastUpdatedByProviderName);
            _fixture.Cohort.LastUpdatedByProviderEmail.Should().Be(expectedLastUpdatedByProviderEmail);
        }

        [TestCase(Party.Employer, "Employer", "foo@foo.com", "Employer", "foo@foo.com", null, null)]
        [TestCase(Party.Provider, "Provider", "bar@bar.com", null, null, "Provider", "bar@bar.com")]
        public void ThenCohortShouldNoLongerBeDraft(Party modifyingParty, string userDisplayName, string userEmail, string expectedLastUpdatedByEmployerName, string expectedLastUpdatedByEmployerEmail, string expectedLastUpdatedByProviderName, string expectedLastUpdatedByProviderEmail)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetUserInfo(userDisplayName, userEmail)
                .SetIsDraft(true)
                .SendToOtherParty();

            _fixture.Cohort.IsDraft.Should().Be(false);
        }


        [TestCase(Party.Employer, typeof(CohortAssignedToProviderEvent))]
        [TestCase(Party.Provider, typeof(CohortAssignedToEmployerEvent))]
        public void ThenShouldPublishEvent(Party modifyingParty, Type expectedEventType)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SendToOtherParty();

            _fixture.UnitOfWorkContext.GetEvents().Single(e => e.GetType() == expectedEventType)
                .Should().BeEquivalentTo(new
                {
                    CohortId = _fixture.Cohort.Id,
                    UpdatedOn = _fixture.Now
                });
        }

        [TestCase(Party.None)]
        [TestCase(Party.TransferSender)]
        public void AndModifyingPartyIsNotEmployerOrProviderThenShouldThrowException(Party modifyingParty)
        {
            _fixture.SetModifyingParty(modifyingParty);
            _fixture.Invoking(f => f.SendToOtherParty()).Should().Throw<DomainException>();
        }

        [TestCase(Party.Employer, Party.Provider, LastAction.Amend)]
        [TestCase(Party.Provider, Party.Employer, LastAction.None)]
        public void AndIsNotWithModifyingPartyThenShouldThrowException(Party modifyingParty, Party withParty, LastAction lastAction)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(withParty)
                .SetLastAction(lastAction);

            _fixture.Invoking(f => f.SendToOtherParty()).Should().Throw<DomainException>();
        }

        [Test]
        public void ThenShouldResetApprovals()
        {
            _fixture.SetModifyingParty(Party.Provider)
                .SetWithParty(Party.Provider)
                .SetApprovals(Party.Employer)
                .SendToOtherParty();
            _fixture.Cohort.Approvals.Should().Be(Party.None);
        }

        [Test]
        public void ThenShouldResetTransferApprovalStatus()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetWithParty(Party.Employer)
                .SetApprovals(Party.Employer)
                .SetTransferApprovalStatus(TransferApprovalStatus.Rejected)
                .SendToOtherParty();

            _fixture.Cohort.TransferApprovalStatus.Should().BeNull();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void ThenShouldPublishEvent(Party modifyingParty)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SendToOtherParty();

            _fixture.VerifyCohortTracking();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void And_IsChangeOfPartyRequest_ThenShouldPublishEvent(Party modifyingParty)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetChangeOfPartyRequestId()
                .SendToOtherParty();

            _fixture.VerifyCohortWithChangeOfPartyRequestEventIsPublished();
        }
    }

    public class WhenSendingCohortToOtherPartyTestsFixture
    {
        public DateTime Now { get; set; }
        public IFixture AutoFixture { get; set; }
        public Party Party { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CommitmentsV2.Models.Cohort Cohort { get; set; }

        public WhenSendingCohortToOtherPartyTestsFixture()
        {
            Now = DateTime.UtcNow;
            AutoFixture = new Fixture();
            Party = Party.None;
            Message = AutoFixture.Create<string>();
            UserInfo = AutoFixture.Create<UserInfo>();
            UnitOfWorkContext = new UnitOfWorkContext();
            Cohort = new CommitmentsV2.Models.Cohort().Set(c => c.Id, 111).Set(x => x.ProviderId, 1);
        }

        public void SendToOtherParty()
        {
            Cohort.SendToOtherParty(Party, Message, UserInfo, Now);
        }

        public WhenSendingCohortToOtherPartyTestsFixture AddDraftApprenticeship()
        {
            ApprenticeshipBase apprenticeship = new DraftApprenticeship(new DraftApprenticeshipDetails(), Party.None);
            Cohort.Add(c => c.Apprenticeships, apprenticeship);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetTransferApprovalStatus(TransferApprovalStatus status)
        {
            Cohort.Set(x => x.TransferApprovalStatus, status);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetWithParty(Party withParty)
        {
            Cohort.Set(c => c.WithParty, withParty);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetLastAction(LastAction lastAction)
        {
            Cohort.Set(c => c.LastAction, lastAction);

            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetApprovals(Party approvals)
        {
            Cohort.Set(c => c.Approvals, approvals);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetIsDraft(bool isDraft)
        {
            Cohort.Set(c => c.IsDraft, isDraft);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetModifyingParty(Party modifyingParty)
        {
            Party = modifyingParty;

            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetUserInfo(string userDisplayName, string userEmail)
        {
            UserInfo.UserDisplayName = userDisplayName;
            UserInfo.UserEmail = userEmail;

            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetChangeOfPartyRequestId()
        {
            Cohort.Set(c => c.ChangeOfPartyRequestId, 123);
            return this;
        }

        public void VerifyCohortTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType ==
                                                                                nameof(Cohort)), Is.Not.Null);
        }

        public void VerifyCohortWithChangeOfPartyRequestEventIsPublished()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortWithChangeOfPartyUpdatedEvent @event
                                                                               && @event.CohortId == Cohort.Id), Is.Not.Null);
        }
    }
}