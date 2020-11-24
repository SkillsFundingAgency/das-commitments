using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    [Parallelizable]
    public class WhenApprovingCohort
    {
        private WhenApprovingCohortFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenApprovingCohortFixture();
        }
        
        [TestCase(Party.Employer, Party.Provider)]
        [TestCase(Party.Provider, Party.Employer)]
        public void AndPartyIsEmployerOrProviderThenShouldUpdateStatus(Party modifyingParty, Party expectedWithParty)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.Cohort.WithParty.Should().Be(expectedWithParty);
            _fixture.Cohort.LastAction.Should().Be(LastAction.Approve);
            _fixture.Cohort.CommitmentStatus.Should().Be(CommitmentStatus.Active);
        }
        
        [TestCase(Party.Employer, EditStatus.Both)]
        [TestCase(Party.Provider, EditStatus.Both)]
        public void AndPartyIsEmployerOrProviderAndOtherPartyHasApprovedThenShouldUpdateStatus(Party modifyingParty, EditStatus expectedEditStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();
            
            _fixture.Cohort.EditStatus.Should().Be(expectedEditStatus);
            _fixture.Cohort.LastAction.Should().Be(LastAction.Approve);
            _fixture.Cohort.CommitmentStatus.Should().Be(CommitmentStatus.Active);
        }

        [TestCase(Party.Employer, null, "", 0)]
        [TestCase(Party.Employer, "Hello Provider", "Hello Provider", 0)]
        [TestCase(Party.Provider, null, "", 1)]
        [TestCase(Party.Provider, "Hello Employer", "Hello Employer", 1)]
        public void AndPartyIsEmployerOrProviderThenShouldAddMessage(Party modifyingParty, string message, string expectedMessage, byte expectedCreatedBy)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .AddDraftApprenticeship()
                .SetMessage(message)
                .Approve();
            
            _fixture.Cohort.Messages.Should().HaveCount(1)
                .And.ContainSingle(m =>
                    m.CreatedBy == expectedCreatedBy &&
                    m.Text == expectedMessage &&
                    m.Author == _fixture.UserInfo.UserDisplayName);
        }
        
        [TestCase(Party.Employer, "Employer", "foo@foo.com", "Employer", "foo@foo.com", null, null)]
        [TestCase(Party.Provider, "Provider", "bar@bar.com", null, null, "Provider", "bar@bar.com")]
        public void AndPartyIsEmployerOrProviderThenShouldSetLastUpdatedBy(Party modifyingParty, string userDisplayName, string userEmail, string expectedLastUpdatedByEmployerName, string expectedLastUpdatedByEmployerEmail, string expectedLastUpdatedByProviderName, string expectedLastUpdatedByProviderEmail)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .AddDraftApprenticeship()
                .SetUserInfo(userDisplayName, userEmail)
                .Approve();
            
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
                .AddDraftApprenticeship()
                .SetUserInfo(userDisplayName, userEmail)
                .SetIsDraft(true)
                .Approve();

            _fixture.Cohort.IsDraft.Should().Be(false);
        }

        [TestCase(Party.Employer, typeof(CohortAssignedToProviderEvent))]
        [TestCase(Party.Provider, typeof(CohortAssignedToEmployerEvent))]
        public void AndPartyIsEmployerOrProviderThenShouldPublishEvent(Party modifyingParty, Type expectedEventType)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().Single(e => e.GetType() == expectedEventType)
                .Should().BeEquivalentTo(new
                {
                    CohortId = _fixture.Cohort.Id,
                    UpdatedOn = _fixture.Now
                });
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsEmployerOrProviderAndOtherPartyHasApprovedThenShouldPublishFullyApprovedEvent(Party modifyingParty)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().OfType<CohortFullyApprovedEvent>()
                .Single().Should().Match<CohortFullyApprovedEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.AccountId == _fixture.Cohort.EmployerAccountId &&
                    e.ProviderId == _fixture.Cohort.ProviderId &&
                    e.UpdatedOn == _fixture.Now);
        }

        [Test]
        public void AndPartyIsEmployerAndProviderHasApprovedThenShouldPublishEmployerApprovedEvent()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetWithParty(Party.Employer)
                .SetApprovals(Party.Provider)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<CohortApprovedByEmployerEvent>()
                .Single()
                .Should()
                .Match<CohortApprovedByEmployerEvent>(e => e.CohortId == _fixture.Cohort.Id &&
                    e.UpdatedOn == _fixture.Now);
        }

        [Test]
        public void AndPartyIsProviderAndEmployerHasApprovedAndCohortIsFundedByTransferThenShouldPublishEvent()
        {
            Party modifyingParty = Party.Provider;

            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetTransferSender()
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();
            
				  _fixture.UnitOfWorkContext.GetEvents()
                .OfType<CohortTransferApprovalRequestedEvent>()
                .Single()
                .Should()
                .Match<CohortTransferApprovalRequestedEvent>(e => e.CohortId == _fixture.Cohort.Id &&
                                                           e.UpdatedOn == _fixture.Now &&
                    e.LastApprovedByParty == modifyingParty);

        }

        [Test]
        public void AndPartyIsEmployerAndProviderHasApprovedAndCohortIsFundedByTransferThenShouldPublishEvents()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetWithParty(Party.Employer)
                .SetTransferSender()
                .SetApprovals(Party.Provider)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents()
                .First(x => x is CohortTransferApprovalRequestedEvent).Should()
                .Match<CohortTransferApprovalRequestedEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.UpdatedOn == _fixture.Now &&
                    e.LastApprovedByParty == Party.Employer
                );
           
            _fixture.UnitOfWorkContext.GetEvents()
                .First(x => x is CohortApprovedByEmployerEvent).Should()
                .Match<CohortApprovedByEmployerEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.UpdatedOn == _fixture.Now 
                );
        }

        [Test]
        public void AndModifyingPartyIsNotEmployerOrProviderOrTransferSenderThenShouldThrowException()
        {
            _fixture.SetModifyingParty(Party.None);
            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer, Party.Provider, LastAction.Amend)]
        [TestCase(Party.Provider, Party.Employer, LastAction.None)]
        public void AndPartyIsEmployerOrProviderAndIsNotWithModifyingPartyThenShouldThrowException(Party modifyingParty, Party withParty, LastAction lastAction)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(withParty)
                .SetLastAction(lastAction);

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsEmployerOrProviderAndHasNoDraftApprenticeshipsThenShouldThrowException(Party party)
        {
            _fixture.SetModifyingParty(party)
                .SetWithParty(party);

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsEmployerOrProviderAndDraftApprenticeshipsAreNotCompleteThenShouldThrowException(Party party)
        {
            _fixture.SetModifyingParty(party)
                .SetWithParty(party)
                .AddDraftApprenticeship(party == Party.Employer, party == Party.Provider);

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [Test]
        public void AndPartyIsTransferSenderAndEmployerAndProviderHaveApprovedAndCohortIsFundedByTransferThenShouldUpdateStatus()
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetWithParty(Party.TransferSender)
                .SetTransferSender()
                .AddDraftApprenticeship()
                .Approve();
            
            _fixture.Cohort.TransferApprovalStatus.Should().Be(TransferApprovalStatus.Approved);
            _fixture.Cohort.TransferApprovalActionedOn.Should().Be(_fixture.Now);
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsTransferSenderAndOtherPartyHasApprovedAndCohortIsFundedByTransferThenShouldPublishEvent(Party modifyingParty)
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetWithParty(Party.TransferSender)
                .SetTransferSender()
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().OfType<CohortFullyApprovedEvent>().Single(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.AccountId == _fixture.Cohort.EmployerAccountId &&
                    e.ProviderId == _fixture.Cohort.ProviderId &&
                    e.UpdatedOn == _fixture.Now);
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsTransferSenderAndIsNotWithModifyingPartyAndCohortIsFundedByTransferThenShouldThrowException(Party withParty)
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetWithParty(withParty)
                .SetTransferSender();

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [Test]
        public void AndPartyIsTransferSenderAndHasNoDraftApprenticeshipsAndCohortIsFundedByTransferThenShouldThrowException()
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetWithParty(Party.TransferSender)
                .SetTransferSender();

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }


        [Test]
        public void AndPartyIsEmployerAndCohortWasRejectedByTransferSenderThenShouldResetTransferApprovalStatus()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetWithParty(Party.Employer)
                .AddDraftApprenticeship()
                .SetTransferApprovalStatus(TransferApprovalStatus.Rejected)
                .Approve();

            _fixture.Cohort.TransferApprovalStatus.Should().BeNull();
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndChangeOfPartyRequestedAndCohortSetWithTransferSenderIdThenShouldSetTransferApprovalStatus(Party modifyingParty)
        {
            _fixture
                .SetChangeOfPartyRequestId()
                .SetTransferSenderId()
                .SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();
            
            _fixture.Cohort.TransferApprovalStatus.Should().Be(TransferApprovalStatus.Approved);
            _fixture.Cohort.WithParty.Should().Be(Party.None);
        }    

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void ThenTheStateChangesAreTracked(Party modifyingParty)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.VerifyCohortTracking();
        }

        [TestCase(Party.Employer, true, true)]
        [TestCase(Party.Employer, false, false)]
        [TestCase(Party.Provider, true, true)]
        [TestCase(Party.Provider, false, false)]
        public void ThenTheEmployerAndProviderApprovedOnDateIsSetCorrectly(Party modifyingParty, bool otherPartyHasApproved, bool expectApprovedOnDate)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(otherPartyHasApproved ? modifyingParty.GetOtherParty(): Party.None)
                .AddDraftApprenticeship()
                .Approve();

            _fixture.VerifyEmployerAndProviderApprovedOnDate(expectApprovedOnDate);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        [TestCase(Party.TransferSender, Description = "Technically at time of writing, a Transfer Cohort cannot be linked to a COPR, but in principal this is still valid")]
        public void ThenIfTheCohortIsLinkedToAChangeOfPartyRequestThenAnEventIsEmitted(Party modifyingParty)
        {
            _fixture
                .SetChangeOfPartyRequestId()                
                .SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(modifyingParty == Party.TransferSender? (Party.Employer | Party.Provider) : modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();

            _fixture.VerifyCohortWithChangeOfPartyRequestFullyApprovedEventIsEmitted(modifyingParty);
        }

        [Test]
        public void AndChangeOfPartyRequestedAndCohortSetWithTransferSenderIdThenShouldNotPublishCohortTransferApprovalRequestedEvent()
        {
            Party modifyingParty = Party.Provider;

            _fixture
                .SetChangeOfPartyRequestId()
                .SetTransferSenderId()
                .SetModifyingParty(modifyingParty)
                .SetWithParty(modifyingParty)
                .SetApprovals(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship()
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().OfType<CohortTransferApprovalRequestedEvent>().Count().Should().Be(0);
        }
    }

    public class WhenApprovingCohortFixture
    {
        public DateTime Now { get; set; }
        public IFixture AutoFixture { get; set; }
        public Party Party { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CommitmentsV2.Models.Cohort Cohort { get; set; }

        public WhenApprovingCohortFixture()
        {
            Now = DateTime.UtcNow;
            AutoFixture = new Fixture();
            Party = Party.None;
            Message = AutoFixture.Create<string>();
            UserInfo = AutoFixture.Create<UserInfo>();
            UnitOfWorkContext = new UnitOfWorkContext();
            
            Cohort = new CommitmentsV2.Models.Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333);
        }

        public void Approve()
        {
            Cohort.Approve(Party, Message, UserInfo, Now);
        }

        public WhenApprovingCohortFixture AddDraftApprenticeship(bool isIncompleteForEmployer = false, bool isIncompleteForProvider = false)
        {
            var draftApprenticeshipDetailsComposer = AutoFixture.Build<DraftApprenticeshipDetails>().WithAutoProperties();
            
            if (isIncompleteForEmployer)
            {
                draftApprenticeshipDetailsComposer = draftApprenticeshipDetailsComposer.Without(d => d.FirstName);
            }
            
            if (isIncompleteForProvider)
            {
                draftApprenticeshipDetailsComposer = draftApprenticeshipDetailsComposer.Without(d => d.Uln);
            }
            
            ApprenticeshipBase apprenticeship = new DraftApprenticeship(draftApprenticeshipDetailsComposer.Create(), Party.Provider);
            Cohort.Add(c => c.Apprenticeships, apprenticeship);
            return this;
        }

        public WhenApprovingCohortFixture SetWithParty(Party withParty)
        {
            Cohort.Set(c => c.WithParty, withParty);
            return this;
        }
        public WhenApprovingCohortFixture SetTransferApprovalStatus(TransferApprovalStatus status)
        {
            Cohort.Set(x => x.TransferApprovalStatus, status);
            return this;
        }

        public WhenApprovingCohortFixture SetLastAction(LastAction lastAction)
        {
            Cohort.Set(c => c.LastAction, lastAction);
            
            return this;
        }

        public WhenApprovingCohortFixture SetModifyingParty(Party modifyingParty)
        {
            Party = modifyingParty;
            
            return this;
        }

        public WhenApprovingCohortFixture SetMessage(string message)
        {
            Message = message;
            
            return this;
        }

        public WhenApprovingCohortFixture SetTransferSender()
        {
            Cohort.Set(c => c.TransferSenderId, 444).Set(c => c.TransferApprovalStatus, TransferApprovalStatus.Pending);
            
            return this;
        }

        public WhenApprovingCohortFixture SetChangeOfPartyRequestId()
        {
            Cohort.Set(c => c.ChangeOfPartyRequestId, 123);
            return this;
        }

        public WhenApprovingCohortFixture SetApprovals(Party approvals)
        {
            Cohort.Set(c => c.Approvals, approvals);
            return this;
        }

        public WhenApprovingCohortFixture SetUserInfo(string userDisplayName, string userEmail)
        {
            UserInfo.UserDisplayName = userDisplayName;
            UserInfo.UserEmail = userEmail;
            
            return this;
        }


        public WhenApprovingCohortFixture SetIsDraft(bool isDraft)
        {
            Cohort.IsDraft = isDraft;
            return this;
        }

        public WhenApprovingCohortFixture SetTransferSenderId()
        {
            Cohort.Set(c => c.TransferSenderId, 456);
            return this;
        }


        public void VerifyCohortTracking()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType ==
                                                                                nameof(Cohort)));
        }

        public void VerifyCohortWithChangeOfPartyRequestFullyApprovedEventIsEmitted(Party modifyingParty)
        {
            var emittedEvent = (CohortWithChangeOfPartyFullyApprovedEvent) UnitOfWorkContext.GetEvents()
                .Single(x => x is CohortWithChangeOfPartyFullyApprovedEvent);

            Assert.AreEqual(Cohort.Id, emittedEvent.CohortId);
            Assert.AreEqual(Cohort.ChangeOfPartyRequestId, emittedEvent.ChangeOfPartyRequestId);
            Assert.AreEqual(UserInfo, emittedEvent.UserInfo);
            Assert.AreEqual(modifyingParty, emittedEvent.ApprovedBy);            
        }

        public void VerifyEmployerAndProviderApprovedOnDate(bool expectValue)
        {
            if (expectValue)
            {
                Assert.AreEqual(DateTime.UtcNow.Date, Cohort.EmployerAndProviderApprovedOn?.Date);
            }
            else
            {
                Assert.IsNull(Cohort.EmployerAndProviderApprovedOn);
            }
        }
    }
}