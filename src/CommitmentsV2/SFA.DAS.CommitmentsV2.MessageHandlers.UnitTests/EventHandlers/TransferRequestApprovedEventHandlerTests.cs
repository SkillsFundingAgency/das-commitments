using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class TransferRequestApprovedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingTransferRequestApprovedEvent_ThenShouldCallMediatorWithCorrectCommand()
        {
            var f = new TransferRequestApprovedEventHandlerTestsFixture();
            await f.Handle();

            f.VerifySend<ApproveCohortCommand>((c, m) =>
                c.CohortId == m.CohortId && c.Message == null &&
                c.UserInfo == m.UserInfo);
        }

        [Test]
        public void Handle_WhenHandlingTransferRequestApprovedEventAndMediatorThrowsException_ThenWelogErrorAndRethrowError()
        {
            var f = new TransferRequestApprovedEventHandlerTestsFixture().SetupMediatorExecuteException();
            Assert.ThrowsAsync<Exception>(() => f.Handle());
            Assert.IsTrue(f.Logger.HasErrors);
        }
    }

    public class TransferRequestApprovedEventHandlerTestsFixture : EventHandlerTestsFixture<TransferRequestApprovedEvent, TransferRequestApprovedEventHandler>
    {
        public FakeLogger<TransferRequestCreatedEvent> Logger { get; set; }

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