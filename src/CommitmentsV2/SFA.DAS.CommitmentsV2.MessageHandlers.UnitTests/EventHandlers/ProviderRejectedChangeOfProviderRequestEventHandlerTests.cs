using AutoFixture;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ProviderRejectedChangeOfProviderRequestEventHandlerTests
    {
        private ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture();
        }

        [Test]
        public async Task WhenHandlingEvent_ThenSendEmailCommandShouldBeSent()
        {
            await _fixture.Handle();

           // _fixture.Mediator.Verify(m => m.Send<It.Is<ProviderRejectedChangeOfProviderRequestCommand>()>)
        }
    }

    class ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public IFixture AutoFixture { get; set; }
        public IHandleMessages<ProviderRejectedChangeOfProviderRequestEvent> Handler { get; set; }
        public ProviderRejectedChangeOfProviderRequestEvent Event { get; set; }

        public ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Handler = new ProviderRejectedChangeOfProviderRequestEventHandler();
            Event = AutoFixture.Create<ProviderRejectedChangeOfProviderRequestEvent>();
        }
        public Task Handle()
        {
            return Handler.Handle(Event, null);
        }
    }
}
