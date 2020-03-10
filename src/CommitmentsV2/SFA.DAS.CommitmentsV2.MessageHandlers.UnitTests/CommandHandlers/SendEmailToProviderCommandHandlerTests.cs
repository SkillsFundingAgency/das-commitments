using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.PAS.Account.Api.ClientV2;
using SFA.DAS.PAS.Account.Api.Types;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    [Parallelizable]
    public class SendEmailToProviderCommandHandlerTests
    {
        private SendEmailToProviderCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new SendEmailToProviderCommandHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_EmailIsSent()
        {
            await _fixture.Handle();
            _fixture.VerifyEmailIsSent();
        }

        [Test]
        public async Task When_HandlingCommand_EmailIsSentToExplicitAddressIfSpecified()
        {
            _fixture.SetExplicitEmailAddress();
            await _fixture.Handle();
            _fixture.VerifyEmailIsSentToExplicitAddress();
        }

        [Test]
        public void When_HandlingCommandAndAnErrorOccurs_AnExceptionIsThrownAndLogged()
        {
            Assert.ThrowsAsync<NullReferenceException>(() => _fixture.SetupNullMessage().Handle());
            _fixture.VerifyHasError();
        }

        private class SendEmailToProviderCommandHandlerTestsFixture
        {
            public SendEmailToProviderCommandHandler Handler;
            public SendEmailToProviderCommand Command;
            public Mock<IPasAccountApiClient> PasAccountApiClient;
            public Mock<IMessageHandlerContext> MessageHandlerContext;
            public FakeLogger<SendEmailToProviderCommandHandler> Logger;

            public long ProviderId;
            public string TemplateId;
            public string ExplicitEmailAddress;
            public Dictionary<string, string> Tokens;

            private Fixture _autoFixture;

            public SendEmailToProviderCommandHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                ProviderId = _autoFixture.Create<long>();
                TemplateId = _autoFixture.Create<string>();
                ExplicitEmailAddress = _autoFixture.Create<string>();
                Tokens = _autoFixture.Create<Dictionary<string, string>>();

                Command = new SendEmailToProviderCommand(ProviderId, TemplateId, Tokens, null);

                MessageHandlerContext = new Mock<IMessageHandlerContext>();
                Logger = new FakeLogger<SendEmailToProviderCommandHandler>();

                PasAccountApiClient = new Mock<IPasAccountApiClient>();
                PasAccountApiClient.Setup(x => x.SendEmailToAllProviderRecipients(It.IsAny<long>(),
                    It.IsAny<ProviderEmailRequest>(), It.IsAny<CancellationToken>()));

                Handler = new SendEmailToProviderCommandHandler(PasAccountApiClient.Object, Logger);
            }

            public SendEmailToProviderCommandHandlerTestsFixture SetExplicitEmailAddress()
            {
                Command = new SendEmailToProviderCommand(ProviderId, TemplateId, Tokens, ExplicitEmailAddress);
                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture SetupNullMessage()
            {
                Command = null;
                return this;
            }

            public async Task Handle()
            {
                await Handler.Handle(Command, MessageHandlerContext.Object);
            }

            public void VerifyEmailIsSent()
            {
                PasAccountApiClient.Verify(x=> x.SendEmailToAllProviderRecipients(It.Is<long>(p => p == ProviderId),
                    It.Is<ProviderEmailRequest>(r =>
                        !r.ExplicitEmailAddresses.Any() && r.TemplateId == TemplateId && r.Tokens == Tokens),
                    It.IsAny<CancellationToken>()));
            }

            public void VerifyEmailIsSentToExplicitAddress()
            {
                PasAccountApiClient.Verify(x => x.SendEmailToAllProviderRecipients(It.Is<long>(p => p == ProviderId),
                    It.Is<ProviderEmailRequest>(r =>
                        r.ExplicitEmailAddresses.Single() == ExplicitEmailAddress
                        && r.TemplateId == TemplateId && r.Tokens == Tokens),
                    It.IsAny<CancellationToken>()));
            }

            public void VerifyHasError()
            {
                Assert.IsTrue(Logger.HasErrors);
            }
        }
    }
}
