using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.UpdatingDraftApprenticeship
{
    public class WhenDraftApprenticeshipIsUpdated
    {
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        //In this test the Cost field is used to represent any field for which updates require the approval
        //of the other party (resulting in the DraftApprenticeship object's AgreementStatus property to be reset)
        //Further tests can be found in: Domain\DraftApprenticeship\WhenDraftApprenticeshipIsUpdated.cs
        public void UpdateDraftApprenticeship_Cost_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_Reference_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipReference();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [Test]
        public void UpdateDraftApprenticeship_Uln_By_Provider_Does_Not_Reset_Employer_Approval()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipUln();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_Tracks_State_Changes(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyDraftApprenticeshipTracking();
            fixture.VerifyCohortTracking();
        }


        private class UpdatingDraftApprenticeshipTestFixture
        {
            private readonly Fixture _autoFixture = new Fixture();
            private UnitOfWorkContext UnitOfWorkContext { get; }
            private UserInfo UserInfo { get; }
            private Party ModifyingParty { get; }
            private CommitmentsV2.Models.Cohort Cohort { get; }
            public Exception Exception { get; private set; }

            public UpdatingDraftApprenticeshipTestFixture(Party modifyingParty)
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                ModifyingParty = modifyingParty;
                Cohort = new CommitmentsV2.Models.Cohort
                {
                    WithParty = modifyingParty,
                    ProviderId = 1
                };

                UserInfo = new UserInfo
                {
                    UserId = "user-1",
                    UserDisplayName = "Tester",
                    UserEmail = "tester@test.com"
                };
            }

            public UpdatingDraftApprenticeshipTestFixture WithExistingDraftApprenticeships()
            {
                Cohort.Apprenticeships.Clear();
                for (var i = 0; i < 10; i++)
                {
                    var apprenticeship = new DraftApprenticeship
                    {
                        Id = i,
                        FirstName = _autoFixture.Create<string>(),
                        LastName = _autoFixture.Create<string>(),
                        Cost = _autoFixture.Create<int>(),
                        CourseCode = _autoFixture.Create<string>(),
                        CourseName = _autoFixture.Create<string>(),
                        DateOfBirth = _autoFixture.Create<DateTime>(),
                    };
                    
                    Cohort.Apprenticeships.Add(apprenticeship);
                }
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithPriorApprovalOfOtherParty()
            {
                Cohort.Approvals = ModifyingParty.GetOtherParty();
                return this;
            }

            public void UpdateDraftApprenticeshipCost()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Cost = details.Cost + 1 ?? 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipReference()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Reference += " modified";
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipUln()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Uln = "3688243446";
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            private DraftApprenticeshipDetails GetRandomApprenticeshipDetailsFromCohort()
            {
                var random = new Random().Next(0, Cohort.Apprenticeships.Count);
                var target = Cohort.Apprenticeships.ToArray()[random];
                var details = ToApprenticeshipDetails(target as DraftApprenticeship);
                return details;
            }

            public void VerifyCohortIsUnapproved()
            {
                Assert.AreEqual(Party.None,Cohort.Approvals);
            }

            public void VerifyCohortIsApprovedByOtherParty()
            {
                Assert.IsTrue(Cohort.Approvals.HasFlag(ModifyingParty.GetOtherParty()));
            }

            public void VerifyDraftApprenticeshipTracking()
            {
                Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(DraftApprenticeship)));
            }

            public void VerifyCohortTracking()
            {
                Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(Cohort)));
            }

            private static DraftApprenticeshipDetails ToApprenticeshipDetails(DraftApprenticeship draftApprenticeship)
            {
                return new DraftApprenticeshipDetails
                {
                    Id = draftApprenticeship.Id,
                    FirstName = draftApprenticeship.FirstName,
                    LastName = draftApprenticeship.LastName,
                    Uln = draftApprenticeship.Uln,
                    TrainingProgramme = new TrainingProgramme(draftApprenticeship.CourseCode, "", ProgrammeType.Framework,
                        null, null),
                    Cost = (int?)draftApprenticeship.Cost,
                    StartDate = draftApprenticeship.StartDate,
                    EndDate = draftApprenticeship.EndDate,
                    DateOfBirth = draftApprenticeship.DateOfBirth,
                    Reference = draftApprenticeship.ProviderRef,
                    ReservationId = draftApprenticeship.ReservationId
                };
            }
        }
    }
}
