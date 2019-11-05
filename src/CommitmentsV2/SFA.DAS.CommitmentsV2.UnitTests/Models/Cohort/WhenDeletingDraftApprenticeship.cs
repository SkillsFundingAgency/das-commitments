using System;
using System.Linq;
using AutoFixture;
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
                Cohort.EditStatus = party.ToEditStatus();
                return this;
            }

            public WhenDeletingDraftApprenticeshipFixture WithApprovalOfOtherParty()
            {
                var otherParty = Cohort.WithParty.GetOtherParty();
                
                foreach (var apprenticeship in Cohort.DraftApprenticeships)
                {
                    apprenticeship.AgreementStatus = otherParty == Party.Employer
                        ? AgreementStatus.EmployerAgreed
                        : AgreementStatus.ProviderAgreed;
                }

                return this;
            }

            public void DeleteDraftApprenticeship()
            {
                //select a target
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

            public void VerifyDeletion()
            {
                Assert.IsFalse(Cohort.DraftApprenticeships.Contains(DeletionTarget));
                Assert.AreEqual(CohortSize-1, Cohort.DraftApprenticeships.Count());
            }

            public void VerifyCohortIsUnapprovedByAllParties()
            {
                Assert.IsTrue(Cohort.Apprenticeships.All(x=> x.AgreementStatus == AgreementStatus.NotAgreed));
            }

            public UserInfo UserInfo { get; }

            private void CreateCohort()
            {
                Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly};

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
