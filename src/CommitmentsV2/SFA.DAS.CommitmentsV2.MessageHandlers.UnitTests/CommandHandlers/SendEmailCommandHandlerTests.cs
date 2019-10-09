using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using Moq;
using MoreLinq.Extensions;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class SendEmailCommandHandlerTests
    {
        private SendEmailCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new SendEmailCommandHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_An_Email_Is_Sent()
        {
            await _fixture.Handle();
            _fixture.VerifyEmailSent();
        }

        [Test]
        public async Task When_HandlingCommand_An_Email_Is_Sent_To_The_Correct_Recipient()
        {
            await _fixture.Handle();
            _fixture.VerifyCorrectRecipient();
        }

        [Test]
        public async Task When_HandlingCommand_An_Email_Is_Sent_With_The_Correct_Template()
        {
            await _fixture.Handle();
            _fixture.VerifyCorrectTemplate();
        }

        [Test]
        public async Task When_HandlingCommand_An_Email_Is_Sent_With_The_Correct_ReplyTo_Address()
        {
            await _fixture.Handle();
            _fixture.VerifyCorrectReplyTo();
        }

        [Test]
        public async Task When_HandlingCommand_An_Email_Is_Sent_With_The_Correct_Tokens()
        {
            await _fixture.Handle();
            _fixture.VerifyCorrectTokens();
        }

        private class SendEmailCommandHandlerTestsFixture
        {
            public SendEmailCommandHandler _handler;
            public SendEmailCommand _command;
            public Mock<INotificationsApi> _notificationsApiClient;

            public readonly string TemplateId;
            public readonly string RecipientsAddress;
            public readonly string ReplyTo;
            public readonly Dictionary<string, string> Tokens;

            public SendEmailCommandHandlerTestsFixture()
            {
                var autoFixture = new Fixture();
                TemplateId = autoFixture.Create<string>();
                RecipientsAddress = autoFixture.Create<string>();
                ReplyTo = autoFixture.Create<string>();
                Tokens = autoFixture.Create<Dictionary<string, string>>();

                _notificationsApiClient = new Mock<INotificationsApi>();
                _notificationsApiClient.Setup(x => x.SendEmail(It.IsAny<Email>())).Returns(() => Task.CompletedTask);

                _command = new SendEmailCommand(TemplateId, RecipientsAddress, ReplyTo, Tokens);

                _handler = new SendEmailCommandHandler(_notificationsApiClient.Object);
            }

            public async Task<SendEmailCommandHandlerTestsFixture> Handle()
            {
                await _handler.Handle(_command, Mock.Of<IMessageHandlerContext>());
                return this;
            }

            public SendEmailCommandHandlerTestsFixture VerifyEmailSent()
            {
                _notificationsApiClient.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Once);
                return this;
            }

            public SendEmailCommandHandlerTestsFixture VerifyCorrectTemplate()
            {
                _notificationsApiClient.Verify(x => x.SendEmail(It.Is<Email>( email => email.TemplateId == TemplateId)), Times.Once);
                return this;
            }

            public SendEmailCommandHandlerTestsFixture VerifyCorrectRecipient()
            {
                _notificationsApiClient.Verify(x => x.SendEmail(It.Is<Email>(email => email.RecipientsAddress == RecipientsAddress)), Times.Once);
                return this;
            }

            public SendEmailCommandHandlerTestsFixture VerifyCorrectReplyTo()
            {
                _notificationsApiClient.Verify(x => x.SendEmail(It.Is<Email>(email => email.ReplyToAddress == ReplyTo)), Times.Once);
                return this;
            }
            public SendEmailCommandHandlerTestsFixture VerifyCorrectTokens()
            {
                var compareLogic = new CompareLogic();
                _notificationsApiClient.Verify(x => x.SendEmail(It.Is<Email>(email =>
                    compareLogic.Compare(Tokens.ToDictionary(), email.Tokens).AreEqual
                    )), Times.Once);
                return this;
            }
        }
    }
}
