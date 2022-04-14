using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [TestCase("test@test.com")]
        public async Task When_HandlingCommand_AndAEmailIsSpecifiedAndUserAcceptsNotifications_OneEmailIsSent(string email)
        {
            _fixture
                .SetupCommandWithSpecificEmailAddress(email)
                .AddToProviderUserList(email, true);

            await _fixture.Handle();

            _fixture.VerifyCorrectMessageSendForSpecificEmail(email);
        }

        [TestCase("test@test.com")]
        public async Task When_HandlingCommand_AndAEmailIsSpecifiedAndNoUserIsFoundInEmployeeList_NoEmailIsSent(string email)
        {
            _fixture
                .SetupCommandWithSpecificEmailAddress(email);

            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent();
        }

        [Test]
        public async Task When_HandlingCommand_AndNoUserFound_NoEmailsAreSent()
        {
            _fixture.SetupCommandWithoutEmailAddress();
            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task When_HandlingCommand_AndTheOnlyUserNotificationsOnHasAEmptyEmail(string email)
        {
            _fixture
                .SetupCommandWithoutEmailAddress()
                .SetupCommandWithSpecificEmailAddress(email);

            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent();
            _fixture.VerifyHasWarning();
        }

        [Test]
        public void When_HandlingCommandAndAnErrorOccurs_AnExceptionIsThrownAndLogged()
        {
            Assert.ThrowsAsync<NullReferenceException>(() => _fixture.SetupNullMessage().Handle());
            _fixture.VerifyHasError();
        }

        [Test]
        public async Task When_HandlingCommand_AndAllUsersHaveNotificationsOn_EmailsAreSentForOwnersAndTransactors()
        {
            _fixture
                .SetupCommandWithoutEmailAddress()
                .AddToProviderUserList();

            await _fixture.Handle();
            _fixture.VerifyMessageAreSent();
        }

        private class SendEmailToProviderCommandHandlerTestsFixture
        {
            public SendEmailToProviderCommandHandler Handler;
            public SendEmailToProviderCommand Command;
            public Mock<IApprovalsOuterApiClient> ApprovalsOuterApiClient;
            public FakeLogger<SendEmailToProviderCommandHandler> Logger;
            public Mock<IMessageHandlerContext> MessageHandlerContext;
            public Mock<IPipelineContext> PipelineContext;

            public long ProviderId;
            public string TemplateId;
            public string ReplyTo;
            public Dictionary<string, string> Tokens;
            public ProvidersUsersResponse ProvidersUser;

            private Fixture _autoFixture;

            public SendEmailToProviderCommandHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                ProviderId = _autoFixture.Create<long>();
                TemplateId = _autoFixture.Create<string>();
                ReplyTo = _autoFixture.Create<string>();
                Tokens = _autoFixture.Create<Dictionary<string, string>>();
                ProvidersUser = _autoFixture.Create<ProvidersUsersResponse>();
                ProvidersUser.Users = new List<GetProviderUsersListItem>();

                Logger = new FakeLogger<SendEmailToProviderCommandHandler>();
                MessageHandlerContext = new Mock<IMessageHandlerContext>();
                PipelineContext = MessageHandlerContext.As<IPipelineContext>();
                ApprovalsOuterApiClient = new Mock<IApprovalsOuterApiClient>();
                ApprovalsOuterApiClient
                    .Setup(x => x.Get<ProvidersUsersResponse>(It.IsAny<GetProviderUsersRequest>()))
                    .ReturnsAsync(ProvidersUser);

                Handler = new SendEmailToProviderCommandHandler(ApprovalsOuterApiClient.Object, Logger);
            }

            public SendEmailToProviderCommandHandlerTestsFixture SetupCommandWithoutEmailAddress()
            {
                Command = new SendEmailToProviderCommand(ProviderId, TemplateId, Tokens);
                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture SetupCommandWithSpecificEmailAddress(string email)
            {
                Command = new SendEmailToProviderCommand(ProviderId, TemplateId, Tokens, email);
                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture SetupNullMessage()
            {
                Command = null;
                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture AddToProviderUserList()
            {
                var users = new List<GetProviderUsersListItem>
                {
                    new GetProviderUsersListItem { ReceiveNotifications = true, EmailAddress = "owner@test.com" },
                    new GetProviderUsersListItem { ReceiveNotifications = true, EmailAddress = "transactor@test.com" },
                    new GetProviderUsersListItem { ReceiveNotifications = true, EmailAddress = "viewer@test.com" }
                };

                ProvidersUser.Users = users;

                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture AddToProviderUserList(string email, bool acceptNotifications)
            {
                var users = new List<GetProviderUsersListItem>
                {
                    new GetProviderUsersListItem { ReceiveNotifications = acceptNotifications, EmailAddress = email }
                };
                ProvidersUser.Users = users;
                return this;
            }

            public SendEmailToProviderCommandHandlerTestsFixture TurnEmployeeNotificationsOff()
            {
                foreach (var user in ProvidersUser.Users)
                {
                    user.ReceiveNotifications = false;
                }
                return this;
            }

            public async Task Handle()
            {
                await Handler.Handle(Command, MessageHandlerContext.Object);
            }

            public void VerifyCorrectMessageSendForSpecificEmail(string email)
            {
                PipelineContext.Verify(x =>
                       x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == email && c.Tokens == Tokens && c.TemplateId == TemplateId),
                        It.IsAny<SendOptions>()), Times.Once);
            }

            public void VerifyNoMessagesSent()
            {
                PipelineContext.Verify(x => x.Publish(It.IsAny<SendEmailCommand>(), It.IsAny<PublishOptions>()), Times.Never);
            }

            public void VerifyMessageAreSent()
            {
                PipelineContext.Verify(x => x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Exactly(3));
                PipelineContext.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == "owner@test.com"), It.IsAny<SendOptions>()), Times.Once);
                PipelineContext.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == "transactor@test.com"), It.IsAny<SendOptions>()), Times.Once);
                PipelineContext.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == "viewer@test.com"), It.IsAny<SendOptions>()), Times.Once);
            }

            public void VerifyHasError()
            {
                Assert.IsTrue(Logger.HasErrors);
            }

            public void VerifyHasWarning()
            {
                Assert.IsTrue(Logger.HasWarnings);
            }
        }
    }
}