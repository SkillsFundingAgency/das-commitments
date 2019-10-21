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
                .SetEditStatus(modifyingParty.ToEditStatus())
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
                .SetEditStatus(modifyingParty.ToEditStatus())
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
                .SetEditStatus(modifyingParty.ToEditStatus())
                .SetUserInfo(userDisplayName, userEmail)
                .SendToOtherParty();
            
            _fixture.Cohort.LastUpdatedByEmployerName.Should().Be(expectedLastUpdatedByEmployerName);
            _fixture.Cohort.LastUpdatedByEmployerEmail.Should().Be(expectedLastUpdatedByEmployerEmail);
            _fixture.Cohort.LastUpdatedByProviderName.Should().Be(expectedLastUpdatedByProviderName);
            _fixture.Cohort.LastUpdatedByProviderEmail.Should().Be(expectedLastUpdatedByProviderEmail);
        }
        
        [TestCase(Party.Employer, typeof(CohortAssignedToProviderEvent))]
        [TestCase(Party.Provider, typeof(CohortAssignedToEmployerEvent))]
        public void ThenShouldPublishEvent(Party modifyingParty, Type expectedEventType)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .SendToOtherParty();
            
            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Single().Should().BeOfType(expectedEventType)
                .And.BeEquivalentTo(new
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
        
        [TestCase(Party.Employer, EditStatus.ProviderOnly, LastAction.Amend)]
        [TestCase(Party.Provider, EditStatus.EmployerOnly, LastAction.None)]
        public void AndIsNotWithModifyingPartyThenShouldThrowException(Party modifyingParty, EditStatus editStatus, LastAction lastAction)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(editStatus)
                .SetLastAction(lastAction);

            _fixture.Invoking(f => f.SendToOtherParty()).Should().Throw<DomainException>();
        }

        [Test]
        public void AndIsApprovedByProviderThenShouldPublishEvent()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetEditStatus(Party.Employer.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.ProviderAgreed)
                .SendToOtherParty();
            
            _fixture.UnitOfWorkContext.GetEvents().OfType<ApprovedCohortReturnedToProviderEvent>().Should().HaveCount(1)
                .And.Subject.Should().ContainSingle(e => e.CohortId == _fixture.Cohort.Id && e.UpdatedOn == _fixture.Now);
        }
        
        [Test]
        public void ThenShouldResetApprovals()
        {
            _fixture.SetModifyingParty(Party.Provider)
                .SetEditStatus(Party.Provider.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.EmployerAgreed)
                .AddDraftApprenticeship(AgreementStatus.EmployerAgreed)
                .SendToOtherParty();

            _fixture.Cohort.Apprenticeships.Should().HaveCount(2)
                .And.Subject.All(a => a.AgreementStatus == AgreementStatus.NotAgreed).Should().BeTrue();
        }

        [Test]
        public void ThenShouldResetTransferApprovalStatus()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetEditStatus(Party.Employer.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.EmployerAgreed)
                .SetTransferApprovalStatus(TransferApprovalStatus.Rejected)
                .SendToOtherParty();

            _fixture.Cohort.TransferApprovalStatus.Should().BeNull();
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
            Cohort = new CommitmentsV2.Models.Cohort().Set(c => c.Id, 111);
        }

        public void SendToOtherParty()
        {
            Cohort.SendToOtherParty(Party, Message, UserInfo, Now);
        }

        public WhenSendingCohortToOtherPartyTestsFixture AddDraftApprenticeship(AgreementStatus agreementStatus)
        {
            Apprenticeship apprenticeship = new DraftApprenticeship(new DraftApprenticeshipDetails(), Party.None).Set(a => a.AgreementStatus, agreementStatus);
            
            Cohort.Add(c => c.Apprenticeships, apprenticeship);
            
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetTransferApprovalStatus(TransferApprovalStatus status)
        {
            Cohort.Set(x => x.TransferApprovalStatus, status);
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetEditStatus(EditStatus editStatus)
        {
            Cohort.Set(c => c.EditStatus, editStatus);
            
            return this;
        }

        public WhenSendingCohortToOtherPartyTestsFixture SetLastAction(LastAction lastAction)
        {
            Cohort.Set(c => c.LastAction, lastAction);
            
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
    }
}