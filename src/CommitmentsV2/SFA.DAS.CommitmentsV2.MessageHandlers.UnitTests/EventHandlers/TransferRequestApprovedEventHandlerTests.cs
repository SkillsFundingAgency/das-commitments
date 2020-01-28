using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TransferRequestApprovedEventHandlerTests : FluentTest<TransferRequestApprovedEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingTransferRequestApprovedEvent_ThenShouldCallMediatorWithCorrectCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<ApproveCohortCommand>((c, m) =>
                c.CohortId == m.CohortId && c.Message == null &&
                c.Party == Party.TransferSender && c.UserInfo == m.UserInfo));
        }

        [Test]
        public Task Handle_WhenHandlingTransferRequestApprovedEventAndMediatorThrowsException_ThenWelogErrorAndRethrowError()
        {
            return TestExceptionAsync(f => f.SetupMediatorExecuteException(), 
                f => f.Handle(), 
                f => f.VerifySend<ApproveCohortCommand>((c, m) =>
                c.CohortId == m.CohortId && c.Message == null &&
                c.Party == Party.TransferSender && c.UserInfo == m.UserInfo));
        }



    }

    public class TransferRequestApprovedEventHandlerTestsFixture : EventHandlerTestsFixture<TransferRequestApprovedEvent, TransferRequestApprovedEventHandler>
    {
        public static ILogger<TransferRequestCreatedEvent> Logger { get; set; }

        public TransferRequestApprovedEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new FakeLogger<TransferRequestCreatedEvent>();
            Handler = new TransferRequestApprovedEventHandler(Mediator.Object, Logger);
        }

        public TransferRequestApprovedEventHandlerTestsFixture SetupMediatorExecuteException()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ApproveCohortCommand>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();
            return this;
        }
    }
}