using System;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    public class WhenDeletingDraftApprenticeship
    {
        private WhenDeletingDraftApprenticeshipFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenDeletingDraftApprenticeshipFixture();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void The_DraftApprenticeship_Is_Removed_From_The_Cohort(Party modifyingParty)
        {
            _fixture
                .WithParty(modifyingParty)
                .DeleteDraftApprenticeship();

            _fixture.VerifyDeletion();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void Any_Existing_Approval_By_Other_Party_Is_Reset(Party modifyingParty)
        {
            _fixture
                .WithParty(modifyingParty)
                .WithApprovalOfOtherParty()
                .DeleteDraftApprenticeship();
            
            _fixture.VerifyCohortIsUnapprovedByAllParties();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void DraftApprenticeshipDeletedEvent_Is_Emitted(Party modifyingParty)
        {
            _fixture
                .WithParty(modifyingParty)
                .DeleteDraftApprenticeship();

            _fixture.VerifyEventEmitted();
        }

        [Test]
        public void Any_Prior_TransferSender_Rejection_Is_Reset()
        {
            _fixture
                .WithParty(Party.Employer)
                .WithTransferSenderRejection()
                .DeleteDraftApprenticeship();

            _fixture.VerifyTransferRejectionReset();
        }

        [Test]
        public void If_The_Cohort_Is_Left_Empty_Then_It_Is_Deleted()
        {
            _fixture
                .WithParty(Party.Employer)
                .WithCohortSize(1)
                .DeleteDraftApprenticeship();

            _fixture.VerifyCohortIsDeleted();
        }

        [Test]
        public void If_The_Cohort_Is_Not_Left_Empty_Then_It_Is_Not_Deleted()
        {
            _fixture
                .WithParty(Party.Employer)
                .WithCohortSize(2)
                .DeleteDraftApprenticeship();

            _fixture.VerifyCohortIsNotDeleted();
        }

        [Test]
        public void If_The_Cohort_Is_Left_Empty_And_Party_Is_Provider_Then_It_Is_Not_Deleted()
        {
            _fixture
                .WithCohortSize(1)
                .WithParty(Party.Provider)
                .DeleteDraftApprenticeship();

            _fixture.VerifyCohortIsNotDeleted();
        }

        private class WhenDeletingDraftApprenticeshipFixture
        {
            public int CohortSize { get; private set; }
            public CommitmentsV2.Models.Cohort Cohort { get; set; }
            public DraftApprenticeship DeletionTarget { get; set; }
            private readonly Fixture _autoFixture = new Fixture();
            public UnitOfWorkContext UnitOfWorkContext { get; set; }

            public WhenDeletingDraftApprenticeshipFixture()
            {
                CohortSize = 10;
                // We need this to allow the UoW to initialise it's internal static events collection.
                UserInfo = _autoFixture.Create<UserInfo>();
                CreateCohort();
                UnitOfWorkContext = new UnitOfWorkContext();
            }

            public WhenDeletingDraftApprenticeshipFixture WithParty(Party party)
            {
                Cohort.WithParty = party;
                return this;
            }

            public WhenDeletingDraftApprenticeshipFixture WithTransferSenderRejection()
            {
                Cohort.TransferApprovalStatus = TransferApprovalStatus.Rejected;
                return this;
            }

            public WhenDeletingDraftApprenticeshipFixture WithApprovalOfOtherParty()
            {
                Cohort.Approvals = Cohort.WithParty.GetOtherParty();
                return this;
            }

            public WhenDeletingDraftApprenticeshipFixture WithCohortSize(int size)
            {
                CohortSize = size;
                CreateCohort();
                return this;
            }

            public void DeleteDraftApprenticeship()
            {
                var minId = (int) Cohort.DraftApprenticeships.Min(x => x.Id);
                var maxId = (int) Cohort.DraftApprenticeships.Max(x => x.Id);
                var randomId = new Random().Next(minId, maxId);
                DeletionTarget = Cohort.DraftApprenticeships.Single(x => x.Id == randomId);

                Cohort.DeleteDraftApprenticeship(DeletionTarget.Id, Cohort.WithParty, UserInfo);
            }

            public void VerifyEventEmitted()
            {
                var emittedEvent = (DraftApprenticeshipDeletedEvent) UnitOfWorkContext.GetEvents().Single(x => x is DraftApprenticeshipDeletedEvent);

                emittedEvent.DraftApprenticeshipId = DeletionTarget.Id;
                emittedEvent.CohortId = Cohort.Id;
                emittedEvent.ReservationId = DeletionTarget.ReservationId;
                emittedEvent.Uln = DeletionTarget.Uln;
            }

            public void VerifyTransferRejectionReset()
            {
                Assert.That(Cohort.TransferApprovalStatus, Is.Null);
                Assert.That(Cohort.LastAction, Is.EqualTo(LastAction.AmendAfterRejected));
            }

            public void VerifyDeletion()
            {
                Assert.That(Cohort.DraftApprenticeships.Contains(DeletionTarget), Is.False);
                Assert.That(Cohort.DraftApprenticeships.Count(), Is.EqualTo(CohortSize - 1));
            }

            public void VerifyCohortIsUnapprovedByAllParties()
            {
                Assert.That(Cohort.Approvals == Party.None, Is.True);
            }

            public void VerifyCohortIsDeleted()
            {
                Assert.That(Cohort.IsDeleted, Is.True);
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortDeletedEvent), Is.Not.Null);
            }

            public void VerifyCohortIsNotDeleted()
            {
                Assert.That(Cohort.IsDeleted, Is.False);
            }

            public UserInfo UserInfo { get; }

            private void CreateCohort()
            {
                Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly, ProviderId = 1};

                for (var i = 0; i < CohortSize; i++)
                {
                    var draftApprenticeship = new DraftApprenticeship
                    {
                        Id = i + 1,
                        FirstName = _autoFixture.Create<string>(),
                        LastName = _autoFixture.Create<string>(),
                        ReservationId = Guid.NewGuid()
                    };

                    Cohort.Apprenticeships.Add(draftApprenticeship);
                }
            }
        }
    }
}
