using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class
        BulkUploadIntoCohortCompletedEventHandlerTests : FluentTest<
            BulkUploadIntoCohortCompletedEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenBulkUploadIntoCohortCompletedEventIsRaised_ThenShouldSendGetDraftApprenticeshipCreatedEventsForCohortQuery()
        {
            return TestAsync(f => f.Handle(), f => f.VerifyQueryIsSent());
        }
        [Test]

        public Task Handle_WhenBulkUploadIntoCohortCompletedEventIsRaised_ThenShouldSendTheDraftApprenticeshipCreatedEventsReturnedInResponse()
        {
            return TestAsync(f => f.Handle(), f => f.VerifyDraftApprenticeshipCreatedEventsArePublished());
        }
    }

    public class BulkUploadIntoCohortCompletedEventHandlerTestsFixture
    {
        public Mock<IMediator> MockMediator { get; set; }
        public Mock<IMessageHandlerContext> MockMessageHandlerContext;
        public BulkUploadIntoCohortCompletedEventHandler BulkUploadIntoCohortCompletedEventHandler;
        public BulkUploadIntoCohortCompletedEvent BulkUploadIntoCohortCompletedEvent;
        public GetDraftApprenticeshipCreatedEventsForCohortResponse GetDraftApprenticeshipCreatedEventsForCohortResponse;

        public BulkUploadIntoCohortCompletedEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            MockMediator = new Mock<IMediator>();
            MockMessageHandlerContext = new Mock<IMessageHandlerContext>();

            BulkUploadIntoCohortCompletedEventHandler =
                new BulkUploadIntoCohortCompletedEventHandler(MockMediator.Object);
            BulkUploadIntoCohortCompletedEvent = autoFixture.Create<BulkUploadIntoCohortCompletedEvent>();
            GetDraftApprenticeshipCreatedEventsForCohortResponse =
                autoFixture.Build<GetDraftApprenticeshipCreatedEventsForCohortResponse>().Create();

            MockMediator.Setup(x => x.Send(It.IsAny<GetDraftApprenticeshipCreatedEventsForCohortQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDraftApprenticeshipCreatedEventsForCohortResponse);
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
            var numberOfEvents = GetDraftApprenticeshipCreatedEventsForCohortResponse.DraftApprenticeshipCreatedEvents.Count();
            var mockPipelineContext = MockMessageHandlerContext.As<IPipelineContext>();
            mockPipelineContext.Verify(x =>x.Publish(It.IsAny<DraftApprenticeshipCreatedEvent>(), It.IsAny<PublishOptions>()), Times.Exactly(numberOfEvents));
        }
    }
}
