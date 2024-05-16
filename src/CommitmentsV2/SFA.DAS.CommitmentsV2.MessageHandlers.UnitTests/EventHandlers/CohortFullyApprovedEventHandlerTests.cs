using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class CohortFullyApprovedEventHandlerTests
    {
        private CohortFullyApprovedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CohortFullyApprovedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendCommand()
        {
            await _fixture.Handle();
            
            _fixture.Mediator.Verify(m => m.Send(
                    It.Is<ProcessFullyApprovedCohortCommand>(c =>
                        c.CohortId == _fixture.Event.CohortId &&
                        c.AccountId == _fixture.Event.AccountId),
                    CancellationToken.None),
                Times.Once);
        }
    }

    public class CohortFullyApprovedEventHandlerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public IFixture AutoFixture { get; set; }
        public IHandleMessages<CohortFullyApprovedEvent> Handler { get; set; }
        public CohortFullyApprovedEvent Event { get; set; }

        public CohortFullyApprovedEventHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Handler = new CohortFullyApprovedEventHandler(Mediator.Object);
            Event = AutoFixture.Create<CohortFullyApprovedEvent>();
        }

        public Task Handle()
        {
            return Handler.Handle(Event, null);
        }
    }
}