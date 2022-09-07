using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    public class WhenResolvingOltdRequest
    {
        private WhenResolvingOltdRequestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenResolvingOltdRequestFixture();
        }

        [Test]
        public void AndRequestResolutionTypeIs_DraftApprenticeshipUpdated_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipUpdated);
            _fixture.UnitOfWorkContext.GetEvents().OfType<OverlappingTrainingDateResolvedEvent>().Count().Should().Be(0);
        }

        [Test]
        public void AndRequestResolutionTypeIs_DraftApprentieshipDeleted_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.DraftApprentieshipDeleted);
            _fixture.UnitOfWorkContext.GetEvents().OfType<OverlappingTrainingDateResolvedEvent>().Count().Should().Be(0);
        }

        [Test]
        public void AndRequestResolutionTypeIs_ApprenticeshipStopped_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<OverlappingTrainingDateResolvedEvent>()
                .Single()
                .Should()
                .Match<OverlappingTrainingDateResolvedEvent>(e =>
                    e.ApprenticeshipId == _fixture.DraftApprenticeshipId && e.CohortId == _fixture.CommitmentId);
        }

        [Test]
        public void AndRequestResolutionTypeIs_ApprenticeshipUpdate_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<OverlappingTrainingDateResolvedEvent>()
                .Single()
                .Should()
                .Match<OverlappingTrainingDateResolvedEvent>(e =>
                    e.ApprenticeshipId == _fixture.DraftApprenticeshipId && e.CohortId == _fixture.CommitmentId);
        }

        [Test]
        public void AndRequestResolutionTypeIs_CompletionDateEvent_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.CompletionDateEvent);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<OverlappingTrainingDateResolvedEvent>()
                .Single()
                .Should()
                .Match<OverlappingTrainingDateResolvedEvent>(e =>
                    e.ApprenticeshipId == _fixture.DraftApprenticeshipId && e.CohortId == _fixture.CommitmentId);
        }

        [Test]
        public void AndRequestResolutionTypeIs_StopDateUpdate_ThenShouldPublishOltdResolvedEvent()
        {
            _fixture.ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType.StopDateUpdate);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<OverlappingTrainingDateResolvedEvent>()
                .Single()
                .Should()
                .Match<OverlappingTrainingDateResolvedEvent>(e =>
                    e.ApprenticeshipId == _fixture.DraftApprenticeshipId && e.CohortId == _fixture.CommitmentId);
        }
    }

    public class WhenResolvingOltdRequestFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }
        public CommitmentsV2.Models.OverlappingTrainingDateRequest OverlappingTrainingDateRequest { get; set; }
        public ICollection<OverlappingTrainingDateRequest> OverlappingTrainingDateRequests { get; set; }
        public DraftApprenticeship DraftApprenticeship { get; set; }
        public long DraftApprenticeshipId { get; set; }
        public long CommitmentId { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public WhenResolvingOltdRequestFixture()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            DraftApprenticeshipId = 111;
            CommitmentId = 222;

            UnitOfWorkContext = new UnitOfWorkContext();

            DraftApprenticeship = fixture.Build<DraftApprenticeship>()
                .With(s => s.CommitmentId, CommitmentId)
                .Without(s => s.Cohort)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.EpaOrg)
                .Without(s => s.PreviousApprenticeship)
                .Without(s => s.EmailAddressConfirmed)
                .Without(s => s.ApprenticeshipConfirmationStatus)
                .Without(s => s.OverlappingTrainingDateRequests)
                .Create();

            OverlappingTrainingDateRequest = new CommitmentsV2.Models.OverlappingTrainingDateRequest()
                .Set(c => c.DraftApprenticeshipId, DraftApprenticeshipId)
                .Set(c => c.Status, OverlappingTrainingDateRequestStatus.Pending)
                .Set(c => c.DraftApprenticeship, DraftApprenticeship);

            OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>();

            OverlappingTrainingDateRequests.Add(OverlappingTrainingDateRequest);

            Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                .Without(s => s.Cohort)
                .With(s => s.OverlappingTrainingDateRequests, OverlappingTrainingDateRequests)
                .Without(s => s.DataLockStatus)
                .Without(s => s.EpaOrg)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.Continuation)
                .Without(s => s.PreviousApprenticeship)
                .Without(s => s.ApprenticeshipConfirmationStatus)
                .Create();

        }

        public void ResolveTrainingDateRequest(OverlappingTrainingDateRequestResolutionType resolutionType)
        {

            Apprenticeship.ResolveTrainingDateRequest(DraftApprenticeshipId, resolutionType);
        }
    }
}
