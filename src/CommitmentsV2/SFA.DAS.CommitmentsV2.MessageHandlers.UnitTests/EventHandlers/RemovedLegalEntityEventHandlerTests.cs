using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Fakes;
using System;
using System.Threading;
using System.Threading.Tasks;
using It = Moq.It;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class RemovedLegalEntityEventHandlerTests : FluentTest<RemovedLegalEntityEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingRemoveLegalEntityEvent_ThenShouldSendRemoveAccountLegalEntityCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<RemoveAccountLegalEntityCommand>((c, m) =>
                c.AccountId == m.AccountId && c.AccountLegalEntityId == m.AccountLegalEntityId && c.Removed == m.Created));
        }

        [Test]
        public Task Handle_WhenHandlingRemoveLegalEntityEvent_AndNoExceptionIsThrown_ThenSuccessInformationIsLogged()
        {
            return TestAsync(
                fixture => fixture.Handle(),
                (fixture) => { fixture.VerifyLoggerHasInformation(); });
        }

        [Test]
        public Task Handle_WhenHandlingRemoveLegalEntityEvent_AndMediatorThrowsException_ThenErrorIsLogged()
        {
            return TestExceptionAsync(
                (fixture) => { fixture.WithMediatorThrowingException(); },
                fixture => fixture.Handle(),
                (fixture, task) =>
                {
                    task.Should().Throw<Exception>();
                    fixture.VerifyLoggerHasErrors();
                });
        }
    }

    public class RemovedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<RemovedLegalEntityEvent, RemovedLegalEntityEventHandler>
    {
        public FakeLogger<RemovedLegalEntityEventHandler> Logger;

        public RemovedLegalEntityEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new FakeLogger<RemovedLegalEntityEventHandler>();

            Handler = new RemovedLegalEntityEventHandler(Mediator.Object, Logger);
        }

        public RemovedLegalEntityEventHandlerTestsFixture WithMediatorThrowingException()
        {
            Mediator
                .Setup(x => x.Send(It.IsAny<RemoveAccountLegalEntityCommand>(), CancellationToken.None))
                .ThrowsAsync(new Exception());
            return this;
        }

        public void VerifyLoggerHasErrors()
        {
            Assert.True(Logger.HasErrors);
        }

        public void VerifyLoggerHasInformation()
        {
            Assert.True(Logger.HasInfo);
        }
    }
}