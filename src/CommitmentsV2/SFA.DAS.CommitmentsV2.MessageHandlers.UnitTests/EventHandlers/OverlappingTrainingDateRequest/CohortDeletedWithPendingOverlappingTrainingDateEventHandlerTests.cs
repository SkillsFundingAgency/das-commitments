using System.Linq;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    public class CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTests
    {
        private CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture();
        }

        [Test]
        public async Task WhenHandlingEvent_If_No_Cohort_Found()
        {
            _fixture.WithNoCohort();
            await _fixture.Handle();
            _fixture.VerifyOverlappingTrainingDateRequestNotResolved();
        }

        [Test]
        public async Task WhenHandlingEvent_If_No_OverlappingTrainingDateRequest_Found()
        {
            _fixture.WithNoOverlappingTrainingDateRequest();
            await _fixture.Handle();
            _fixture.VerifyOverlappingTrainingDateRequestNotResolved();
        }

        [Test]
        public async Task WhenHandlingEvent_If_OverlappingTrainingDateRequest_IsNotPending()
        {
            _fixture.WithResolvedOverlappingTrainingDateRequest();
            await _fixture.Handle();
            _fixture.VerifyOverlappingTrainingDateRequestNotResolved();
        }

        [Test]
        public async Task WhenHandlingEvent_If_OverlappingTrainingDateRequest_IsPending()
        {
            await _fixture.Handle();
            _fixture.VerifyOverlappingTrainingDateRequestResolved();
        }

        private class CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture
        {
            private readonly CohortDeletedWithPendingOverlappingTrainingDateEventHandler _handler;
            private readonly Mock<ProviderCommitmentsDbContext> _db;
            private readonly CohortDeletedEvent _event;
            private readonly Cohort _cohort;
            private readonly Models.OverlappingTrainingDateRequest _overlappingTrainingDateRequest;
            private readonly Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private readonly Apprenticeship _apprenticeship;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;

            public CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();
                _event = autoFixture.Create<CohortDeletedEvent>();

                _overlappingTrainingDateRequest = autoFixture.Create<Models.OverlappingTrainingDateRequest>();

                _apprenticeship = new Apprenticeship();
                _apprenticeship.SetValue(x => x.OverlappingTrainingDateRequests,
                    new List<Models.OverlappingTrainingDateRequest> { _overlappingTrainingDateRequest });

                _cohort = new Cohort();
                _cohort.SetValue(x => x.Id, _event.CohortId);
                _cohort.SetValue(x => x.Apprenticeships, new List<ApprenticeshipBase> { _apprenticeship });

                _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
                _db.Setup(context => context.Cohorts)
                    .ReturnsDbSet([_cohort]);

                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _resolveOverlappingTrainingDateRequestService.Setup(x => x.Resolve(
                    It.IsAny<long?>(),
                    It.IsAny<long?>(),
                    It.IsAny<OverlappingTrainingDateRequestResolutionType>()
                )).Returns(Task.CompletedTask);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new CohortDeletedWithPendingOverlappingTrainingDateEventHandler(
                    new Lazy<ProviderCommitmentsDbContext>(() => _db.Object),
                    _resolveOverlappingTrainingDateRequestService.Object,
                    Mock.Of<ILogger<CohortDeletedWithPendingOverlappingTrainingDateEventHandler>>());
            }

            public CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture WithNoCohort()
            {
                _db
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet([]);
                return this;
            }

            public CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture WithNoOverlappingTrainingDateRequest()
            {
                _cohort.Apprenticeships.FirstOrDefault().OverlappingTrainingDateRequests = null;
                _db
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet([_cohort]);
                return this;
            }

            public CohortDeletedWithPendingOverlappingTrainingDateEventHandlerTestsFixture WithResolvedOverlappingTrainingDateRequest()
            {
                _cohort.Apprenticeships.FirstOrDefault().OverlappingTrainingDateRequests.FirstOrDefault().Status = OverlappingTrainingDateRequestStatus.Resolved;
                _db
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet([_cohort]);
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
                _db.Object.SaveChanges();
            }

            public void VerifyOverlappingTrainingDateRequestNotResolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_overlappingTrainingDateRequest.PreviousApprenticeshipId,
                    _overlappingTrainingDateRequest.DraftApprenticeshipId, OverlappingTrainingDateRequestResolutionType.CohortDeleted), Times.Never);
            }

            public void VerifyOverlappingTrainingDateRequestResolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_overlappingTrainingDateRequest.PreviousApprenticeshipId,
                    null, OverlappingTrainingDateRequestResolutionType.CohortDeleted), Times.Once);
            }
        }
    }
}
