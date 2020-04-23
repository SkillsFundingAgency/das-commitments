using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Fakes;
using It = Moq.It;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class AddedLegalEntityEventHandlerTests : FluentTest<AddedLegalEntityEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingAddedLegalEntityEvent_ThenShouldSendAddAccountLegalEntityCommand()
        {
            return TestAsync(
                f => f.Handle(),
                f => f.VerifySend<AddAccountLegalEntityCommand>((c, m) =>
                  c.AccountId == m.AccountId &&
                  c.AccountLegalEntityId == m.AccountLegalEntityId &&
                  c.MaLegalEntityId == m.LegalEntityId &&
                  c.AccountLegalEntityPublicHashedId == m.AccountLegalEntityPublicHashedId &&
                  c.OrganisationName == m.OrganisationName &&
                  c.Created == m.Created));
        }

        [Test]
        public Task Handle_WhenHandlingAddedLegalEntityEvent_AndNoExceptionIsThrown_ThenSuccessInformationIsLogged()
        {
            return TestAsync(
                fixture => fixture.Handle(),
                (fixture) => { fixture.VerifyLoggerHasInformation();});
        }

        [Test]
        public Task Handle_WhenHandlingAddedLegalEntityEvent_AndMediatorThrowsException_ThenErrorIsLogged()
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

    public class AddedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<AddedLegalEntityEvent, AddedLegalEntityEventHandler>
    {
        public FakeLogger<AddedLegalEntityEventHandler> Logger;

        public AddedLegalEntityEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new FakeLogger<AddedLegalEntityEventHandler>();

            Handler = new AddedLegalEntityEventHandler(Mediator.Object, Logger);
        }

        public AddedLegalEntityEventHandlerTestsFixture WithMediatorThrowingException()
        {
            Mediator
                .Setup(x => x.Send(It.IsAny<AddAccountLegalEntityCommand>(), CancellationToken.None))
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