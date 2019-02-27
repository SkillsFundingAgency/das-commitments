﻿using System;
using AutoFixture;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NServiceBus;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests
{
    public class EventHandlerTestsFixture<TEvent, TEventHandler> where TEventHandler : IHandleMessages<TEvent>
    {
        public Mock<IMediator> Mediator { get; set; }
        public TEvent Message { get; set; }
        public IHandleMessages<TEvent> Handler { get; set; }
        public string MessageId { get; set; }
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }

        public EventHandlerTestsFixture(Func<IMediator, IHandleMessages<TEvent>> constructHandler = null)
        {
            Mediator = new Mock<IMediator>();

            var fixture = new Fixture();
            Message = fixture.Create<TEvent>();

            MessageId = fixture.Create<string>();
            MessageHandlerContext = new Mock<IMessageHandlerContext>();

            MessageHandlerContext.Setup(c => c.MessageId).Returns(MessageId);

            Handler = constructHandler != null ? constructHandler(Mediator.Object) : ConstructHandler();
        }

        public virtual Task Handle()
        {
            return Handler.Handle(Message, MessageHandlerContext.Object);
        }

        private TEventHandler ConstructHandler()
        {
            return (TEventHandler)Activator.CreateInstance(typeof(TEventHandler), Mediator.Object);
        }

        public void VerifySend<TCommand>(Func<TCommand, TEvent, bool> verifyCommand) where TCommand : IRequest
        {
            Mediator.Verify(m => m.Send(It.Is<TCommand>(c => verifyCommand(c, Message)), CancellationToken.None), Times.Once);
        }
    }
}
