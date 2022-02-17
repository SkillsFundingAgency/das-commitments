using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipResendInvitationCommandHandlerTests
    {
        private ApprenticeshipResendInvitationCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipResendInvitationCommandHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_IsSentToApprenticeCommitments()
        {
            await _fixture.Handle();
            _fixture.VerifyApprenticeCommitmentsCommandIsSent();
        }

        [Test]
        public void When_HandlingCommandAndAnErrorOccurs_AnExceptionIsThrownAndLogged()
        {
            Assert.ThrowsAsync<Exception>(() => _fixture.SendingApprenticeCommitmentsCommandFails().Handle());
            _fixture.VerifyHasError();
        }

        private class ApprenticeshipResendInvitationCommandHandlerTestsFixture
        {
            public ApprenticeshipResendInvitationCommandHandler Handler;
            public ApprenticeshipResendInvitationCommand Command;
            public Mock<IMessageHandlerContext> MessageHandlerContext;
            public FakeLogger<ApprenticeshipResendInvitationCommandHandler> Logger;

            private Fixture _autoFixture;

            public ApprenticeshipResendInvitationCommandHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                Command = _autoFixture.Create<ApprenticeshipResendInvitationCommand>();

                MessageHandlerContext = new Mock<IMessageHandlerContext>();
                Logger = new FakeLogger<ApprenticeshipResendInvitationCommandHandler>();

                Handler = new ApprenticeshipResendInvitationCommandHandler(Logger);
            }

            public ApprenticeshipResendInvitationCommandHandlerTestsFixture SendingApprenticeCommitmentsCommandFails()
            {
                MessageHandlerContext
                    .Setup(x => x.Send(It.IsAny<SendApprenticeshipInvitationCommand>(), It.IsAny<SendOptions>()))
                    .ThrowsAsync(new Exception("Failed to send"));
                return this;
            }

            public async Task Handle()
            {
                await Handler.Handle(Command, MessageHandlerContext.Object);
            }

            public void VerifyApprenticeCommitmentsCommandIsSent()
            {
                MessageHandlerContext.Verify(x => x.Send(It.Is<SendApprenticeshipInvitationCommand>(p =>
                    p.CommitmentsApprenticeshipId == Command.ApprenticeshipId && p.ResendOn == Command.ResendOn), It.IsAny<SendOptions>()));
            }

            public void VerifyHasError()
            {
                Assert.IsTrue(Logger.HasErrors);
            }
        }
    }
}
