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
        
        [TestCase(Party.Employer, EditStatus.ProviderOnly, AgreementStatus.EmployerAgreed)]
        [TestCase(Party.Provider, EditStatus.EmployerOnly, AgreementStatus.ProviderAgreed)]
        public void AndPartyIsEmployerOrProviderThenShouldUpdateStatus(Party modifyingParty, EditStatus expectedEditStatus, AgreementStatus expectedAgreementStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.NotAgreed)
                .Approve();
            
            _fixture.Cohort.EditStatus.Should().Be(expectedEditStatus);
            _fixture.Cohort.LastAction.Should().Be(LastAction.Approve);
            _fixture.Cohort.CommitmentStatus.Should().Be(CommitmentStatus.Active);
            _fixture.Cohort.DraftApprenticeships.Should().OnlyContain(a => a.AgreementStatus == expectedAgreementStatus && a.AgreedOn == null);
        }
        
        [TestCase(Party.Employer, AgreementStatus.ProviderAgreed, EditStatus.Both)]
        [TestCase(Party.Provider, AgreementStatus.EmployerAgreed, EditStatus.Both)]
        public void AndPartyIsEmployerOrProviderAndOtherPartyHasApprovedThenShouldUpdateStatus(Party modifyingParty, AgreementStatus agreementStatus, EditStatus expectedEditStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(agreementStatus)
                .Approve();
            
            _fixture.Cohort.EditStatus.Should().Be(expectedEditStatus);
            _fixture.Cohort.LastAction.Should().Be(LastAction.Approve);
            _fixture.Cohort.CommitmentStatus.Should().Be(CommitmentStatus.Active);
            _fixture.Cohort.DraftApprenticeships.Should().OnlyContain(a => a.AgreementStatus == AgreementStatus.BothAgreed && a.AgreedOn == _fixture.Now);
        }

        [TestCase(Party.Employer, null, "", 0)]
        [TestCase(Party.Employer, "Hello Provider", "Hello Provider", 0)]
        [TestCase(Party.Provider, null, "", 1)]
        [TestCase(Party.Provider, "Hello Employer", "Hello Employer", 1)]
        public void AndPartyIsEmployerOrProviderThenShouldAddMessage(Party modifyingParty, string message, string expectedMessage, byte expectedCreatedBy)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.NotAgreed)
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
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.NotAgreed)
                .SetUserInfo(userDisplayName, userEmail)
                .Approve();
            
            _fixture.Cohort.LastUpdatedByEmployerName.Should().Be(expectedLastUpdatedByEmployerName);
            _fixture.Cohort.LastUpdatedByEmployerEmail.Should().Be(expectedLastUpdatedByEmployerEmail);
            _fixture.Cohort.LastUpdatedByProviderName.Should().Be(expectedLastUpdatedByProviderName);
            _fixture.Cohort.LastUpdatedByProviderEmail.Should().Be(expectedLastUpdatedByProviderEmail);
        }
        
        [TestCase(Party.Employer, typeof(CohortAssignedToProviderEvent))]
        [TestCase(Party.Provider, typeof(CohortAssignedToEmployerEvent))]
        public void AndPartyIsEmployerOrProviderThenShouldPublishEvent(Party modifyingParty, Type expectedEventType)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.NotAgreed)
                .Approve();
            
            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Single().Should().BeOfType(expectedEventType)
                .And.BeEquivalentTo(new
                {
                    CohortId = _fixture.Cohort.Id,
                    UpdatedOn = _fixture.Now
                });
        }
        
        [TestCase(Party.Employer, AgreementStatus.ProviderAgreed)]
        [TestCase(Party.Provider, AgreementStatus.EmployerAgreed)]
        public void AndPartyIsEmployerOrProviderAndOtherPartyHasApprovedThenShouldPublishFullyApprovedEvent(Party modifyingParty, AgreementStatus agreementStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .AddDraftApprenticeship(agreementStatus)
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().Should().Subject.LastOrDefault()
                .Should().Match<CohortFullyApprovedEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.AccountId == _fixture.Cohort.EmployerAccountId &&
                    e.ProviderId == _fixture.Cohort.ProviderId.Value &&
                    e.UpdatedOn == _fixture.Now);
        }

        [Test]
        public void AndPartyIsEmployerAndProviderHasApprovedThenShouldPublishEmployerApprovedEvent()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetEditStatus(EditStatus.EmployerOnly)
                .AddDraftApprenticeship(AgreementStatus.ProviderAgreed)
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(2)
                .And.Subject.FirstOrDefault().Should().Match<CohortApprovedByEmployerEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.UpdatedOn == _fixture.Now);
        }

        [TestCase(Party.Employer, AgreementStatus.ProviderAgreed)]
        [TestCase(Party.Provider, AgreementStatus.EmployerAgreed)]
        public void AndPartyIsEmployerOrProviderAndOtherPartyHasApprovedAndCohortIsFundedByTransferThenShouldPublishEvent(Party modifyingParty, AgreementStatus agreementStatus)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(modifyingParty.ToEditStatus())
                .SetTransferSender()
                .AddDraftApprenticeship(agreementStatus)
                .Approve();
            
            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Single().Should().Match<CohortTransferApprovalRequestedEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.UpdatedOn == _fixture.Now &&
                    e.LastApprovedByParty == modifyingParty);
        }
        
        [Test]
        public void AndModifyingPartyIsNotEmployerOrProviderOrTransferSenderThenShouldThrowException()
        {
            _fixture.SetModifyingParty(Party.None);
            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer, EditStatus.ProviderOnly, LastAction.Amend)]
        [TestCase(Party.Provider, EditStatus.EmployerOnly, LastAction.None)]
        public void AndPartyIsEmployerOrProviderAndIsNotWithModifyingPartyThenShouldThrowException(Party modifyingParty, EditStatus editStatus, LastAction lastAction)
        {
            _fixture.SetModifyingParty(modifyingParty)
                .SetEditStatus(editStatus)
                .SetLastAction(lastAction);

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsEmployerOrProviderAndHasNoDraftApprenticeshipsThenShouldThrowException(Party party)
        {
            _fixture.SetModifyingParty(party)
                .SetEditStatus(party.ToEditStatus());

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void AndPartyIsEmployerOrProviderAndDraftApprenticeshipsAreNotCompleteThenShouldThrowException(Party party)
        {
            _fixture.SetModifyingParty(party)
                .SetEditStatus(party.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.NotAgreed, party == Party.Employer, party == Party.Provider);

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [Test]
        public void AndPartyIsTransferSenderAndEmployerAndProviderHaveApprovedAndCohortIsFundedByTransferThenShouldUpdateStatus()
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetEditStatus(EditStatus.Both)
                .SetTransferSender()
                .AddDraftApprenticeship(AgreementStatus.BothAgreed)
                .Approve();
            
            _fixture.Cohort.TransferApprovalStatus.Should().Be(TransferApprovalStatus.Approved);
            _fixture.Cohort.TransferApprovalActionedOn.Should().Be(_fixture.Now);
        }
        
        [TestCase(Party.Employer, AgreementStatus.ProviderAgreed)]
        [TestCase(Party.Provider, AgreementStatus.EmployerAgreed)]
        public void AndPartyIsTransferSenderAndOtherPartyHasApprovedAndCohortIsFundedByTransferThenShouldPublishEvent(Party modifyingParty, AgreementStatus agreementStatus)
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetEditStatus(EditStatus.Both)
                .SetTransferSender()
                .AddDraftApprenticeship(AgreementStatus.BothAgreed)
                .Approve();

            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Single().Should().Match<CohortFullyApprovedEvent>(e =>
                    e.CohortId == _fixture.Cohort.Id &&
                    e.AccountId == _fixture.Cohort.EmployerAccountId &&
                    e.ProviderId == _fixture.Cohort.ProviderId.Value &&
                    e.UpdatedOn == _fixture.Now);
        }
        
        [TestCase(EditStatus.EmployerOnly)]
        [TestCase(EditStatus.ProviderOnly)]
        public void AndPartyIsTransferSenderAndIsNotWithModifyingPartyAndCohortIsFundedByTransferThenShouldThrowException(EditStatus editStatus)
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetEditStatus(editStatus)
                .SetTransferSender();

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }
        
        [Test]
        public void AndPartyIsTransferSenderAndHasNoDraftApprenticeshipsAndCohortIsFundedByTransferThenShouldThrowException()
        {
            _fixture.SetModifyingParty(Party.TransferSender)
                .SetEditStatus(EditStatus.Both)
                .SetTransferSender();

            _fixture.Invoking(f => f.Approve()).Should().Throw<DomainException>();
        }


        [Test]
        public void AndPartyIsEmployerAndCohortWasRejectedByTransferSenderThenShouldResetTransferApprovalStatus()
        {
            _fixture.SetModifyingParty(Party.Employer)
                .SetEditStatus(Party.Employer.ToEditStatus())
                .AddDraftApprenticeship(AgreementStatus.EmployerAgreed)
                .SetTransferApprovalStatus(TransferApprovalStatus.Rejected)
                .Approve();

            _fixture.Cohort.TransferApprovalStatus.Should().BeNull();
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

        public WhenApprovingCohortFixture AddDraftApprenticeship(AgreementStatus agreementStatus, bool isIncompleteForEmployer = false, bool isIncompleteForProvider = false)
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
            
            Apprenticeship apprenticeship = new DraftApprenticeship(draftApprenticeshipDetailsComposer.Create(), Party.Provider).Set(a => a.AgreementStatus, agreementStatus);
            
            Cohort.Add(c => c.Apprenticeships, apprenticeship);
            
            return this;
        }

        public WhenApprovingCohortFixture SetEditStatus(EditStatus editStatus)
        {
            Cohort.Set(c => c.EditStatus, editStatus);
            
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

        public WhenApprovingCohortFixture SetUserInfo(string userDisplayName, string userEmail)
        {
            UserInfo.UserDisplayName = userDisplayName;
            UserInfo.UserEmail = userEmail;
            
            return this;
        }
    }
}