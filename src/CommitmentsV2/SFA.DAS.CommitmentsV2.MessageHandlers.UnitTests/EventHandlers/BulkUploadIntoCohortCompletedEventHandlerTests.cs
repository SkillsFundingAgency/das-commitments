using System.Linq;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class
        BulkUploadIntoCohortCompletedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenBulkUploadIntoCohortCompletedEventIsRaised_ThenShouldSendGetDraftApprenticeshipCreatedEventsForCohortQuery()
        {
            var fixture = new BulkUploadIntoCohortCompletedEventHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyQueryIsSent();
        }
        [Test]

        public async Task Handle_WhenBulkUploadIntoCohortCompletedEventIsRaised_ThenShouldSendTheDraftApprenticeshipCreatedEventsReturnedInResponse()
        {
            var fixture = new BulkUploadIntoCohortCompletedEventHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyDraftApprenticeshipCreatedEventsArePublished();
        }
    }

    public class BulkUploadIntoCohortCompletedEventHandlerTestsFixture
    {
        public Mock<IMediator> MockMediator { get; set; }
        public Mock<IMessageHandlerContext> MockMessageHandlerContext;
        public BulkUploadIntoCohortCompletedEventHandler BulkUploadIntoCohortCompletedEventHandler;
        public BulkUploadIntoCohortCompletedEvent BulkUploadIntoCohortCompletedEvent;
        public GetDraftApprenticeshipCreatedEventsForCohortQueryResult GetDraftApprenticeshipCreatedEventsForCohortQueryResult;

        public BulkUploadIntoCohortCompletedEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            MockMediator = new Mock<IMediator>();
            MockMessageHandlerContext = new Mock<IMessageHandlerContext>();

            BulkUploadIntoCohortCompletedEventHandler =
                new BulkUploadIntoCohortCompletedEventHandler(MockMediator.Object);
            BulkUploadIntoCohortCompletedEvent = autoFixture.Create<BulkUploadIntoCohortCompletedEvent>();
            GetDraftApprenticeshipCreatedEventsForCohortQueryResult =
                autoFixture.Build<GetDraftApprenticeshipCreatedEventsForCohortQueryResult>().Create();

            MockMediator.Setup(x => x.Send(It.IsAny<GetDraftApprenticeshipCreatedEventsForCohortQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDraftApprenticeshipCreatedEventsForCohortQueryResult);
        }

        public Task Handle()
        {
            return BulkUploadIntoCohortCompletedEventHandler.Handle(BulkUploadIntoCohortCompletedEvent,
                MockMessageHandlerContext.Object);
        }

        public void VerifyQueryIsSent()
        {
            var e = BulkUploadIntoCohortCompletedEvent;
            MockMediator.Verify(m => m.Send(It.Is<GetDraftApprenticeshipCreatedEventsForCohortQuery>(q =>
                q.ProviderId == e.ProviderId &&
                q.CohortId == e.CohortId &&
                q.NumberOfApprentices == e.NumberOfApprentices &&
                q.UploadedOn == e.UploadedOn), CancellationToken.None));
        }

        public void VerifyDraftApprenticeshipCreatedEventsArePublished()
        {
            var numberOfEvents = GetDraftApprenticeshipCreatedEventsForCohortQueryResult.DraftApprenticeshipCreatedEvents.Count();
            var mockPipelineContext = MockMessageHandlerContext.As<IPipelineContext>();
            mockPipelineContext.Verify(x =>x.Publish(It.IsAny<DraftApprenticeshipCreatedEvent>(), It.IsAny<PublishOptions>()), Times.Exactly(numberOfEvents));
        }
    }
}
