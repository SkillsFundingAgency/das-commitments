using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    [Parallelizable]
    public class SendEmailToEmployerCommandHandlerTests
    {
        private SendEmailToEmployerCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new SendEmailToEmployerCommandHandlerTestsFixture();
        }

        [TestCase("test@test.com")]
        public async Task When_HandlingCommand_AndAEmailIsSpecifiedAndUserAcceptsNotifications_OneEmailIsSent(string email)
        {
            _fixture.SetupCommandWithSpecificEmailAddress(email).AddToEmployeeList(email.ToUpper(), true);
            await _fixture.Handle();
            _fixture.VerifyCorrectMessageSendForSpecificEmail(email);
        }

        [TestCase("test@test.com")]
        public async Task When_HandlingCommand_AndAEmailIsSpecifiedAndUserDoesntAcceptsNotifications_NoEmailIsSent(string email)
        {
            _fixture.SetupCommandWithSpecificEmailAddress(email).AddToEmployeeList(email, false);
            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent(); 
        }

        [TestCase("test@test.com")]
        public async Task When_HandlingCommand_AndAEmailIsSpecifiedAndNoUserIsFoundInEmployeeList_NoEmailIsSent(string email)
        {
            _fixture.SetupCommandWithSpecificEmailAddress(email);
            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent(); 
        }

        [Test]
        public async Task When_HandlingCommand_AndNoEmployersFound_NoEmailsAreSent()
        {
            _fixture.SetupCommandWithoutEmailAddress();
            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent();
        }

        [Test]
        public async Task When_HandlingCommand_AndAllUsersHaveTurnedNotificationsOff_NoEmailsAreSent()
        {
            _fixture.SetupCommandWithoutEmailAddress().AddToEmployeeListOneOwnerOneTransactorAndOneViewer().TurnEmployeeNotificationsOff();
            await _fixture.Handle();
            _fixture.VerifyNoMessagesSent();
        }

        [Test]
        public async Task When_HandlingCommand_AndAllUsersHaveNotificationsOn_EmailsAreSentForOwnersAndTransactors()
        {
            _fixture.SetupCommandWithoutEmailAddress().AddToEmployeeListOneOwnerOneTransactorAndOneViewer();
            await _fixture.Handle();
            _fixture.VerifyMessageAreSentForOwnersAndTransactors();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task When_HandlingCommand_AndTheOnlyUserNotificationsOnHasAEmptyEmail_EmailsAreSentForOwnersAndTransactors(string email)
        {
            _fixture.SetupCommandWithoutEmailAddress().AddEmployeeOwnerWithEmail(email);
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

        private class SendEmailToEmployerCommandHandlerTestsFixture
        {
            public SendEmailToEmployerCommandHandler Handler;
            public SendEmailToEmployerCommand Command;
            public Mock<IAccountApiClient> AccountApiClient;
            public FakeLogger<SendEmailToEmployerCommandHandler> Logger;
            public Mock<IMessageHandlerContext> MessageHandlerContext;
            public Mock<IPipelineContext> PipelineContext;

            public long AccountId;
            public string TemplateId;
            public string ReplyTo;
            public Dictionary<string, string> Tokens;
            public List<TeamMemberViewModel> Employees;

            private Fixture _autoFixture;

            public SendEmailToEmployerCommandHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                AccountId = _autoFixture.Create<long>();
                TemplateId = _autoFixture.Create<string>();
                ReplyTo = _autoFixture.Create<string>();
                Tokens = _autoFixture.Create<Dictionary<string, string>>();
                Employees = _autoFixture.CreateMany<TeamMemberViewModel>().ToList();

                Logger = new FakeLogger<SendEmailToEmployerCommandHandler>();
                MessageHandlerContext = new Mock<IMessageHandlerContext>();
                PipelineContext = MessageHandlerContext.As<IPipelineContext>();
                AccountApiClient = new Mock<IAccountApiClient>();
                AccountApiClient.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(Employees);

                Handler = new SendEmailToEmployerCommandHandler(AccountApiClient.Object, Logger);
            }

            public SendEmailToEmployerCommandHandlerTestsFixture SetupCommandWithoutEmailAddress()
            {
                Command = new SendEmailToEmployerCommand(AccountId, TemplateId, Tokens);
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture SetupCommandWithSpecificEmailAddress(string email)
            {
                Command = new SendEmailToEmployerCommand(AccountId, TemplateId, Tokens, email);
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture SetupNullMessage()
            {
                Command = null;
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture AddToEmployeeListOneOwnerOneTransactorAndOneViewer()
            {
                Employees.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "owner@test.com", Role = "Owner" });
                Employees.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "transactor@test.com", Role = "Transactor" });
                Employees.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "viewer@test.com", Role = "Viewer" });
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture AddToEmployeeList(string email, bool acceptNotifications)
            {
                Employees.Add(new TeamMemberViewModel { CanReceiveNotifications = acceptNotifications, Email = email, Role = "Viewer" });
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture TurnEmployeeNotificationsOff()
            {
                foreach (var employee in Employees)
                {
                    employee.CanReceiveNotifications = false;
                }
                return this;
            }

            public SendEmailToEmployerCommandHandlerTestsFixture AddEmployeeOwnerWithEmail(string email)
            {
                Employees.Add(new TeamMemberViewModel {CanReceiveNotifications = true, Email = email, Role = "Owner"});
                return this;
            }

            public async Task Handle()
            {
                await Handler.Handle(Command, MessageHandlerContext.Object);
            }

            public void VerifyCorrectMessageSendForSpecificEmail(string email)
            {
                PipelineContext.Verify(
                    x => x.Send(It.Is<SendEmailCommand>(c =>
                        c.RecipientsAddress == email && c.Tokens == Tokens && c.TemplateId == TemplateId), It.IsAny<SendOptions>()), Times.Once);
            }

            public void VerifyNoMessagesSent()
            {
                PipelineContext.Verify(x => x.Publish(It.IsAny<SendEmailCommand>(), It.IsAny<PublishOptions>()), Times.Never);
            }

            public void VerifyMessageAreSentForOwnersAndTransactors()
            {
                PipelineContext.Verify(x=>x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Exactly(2));
                PipelineContext.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == "owner@test.com"), It.IsAny<SendOptions>()), Times.Once);
                PipelineContext.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress == "transactor@test.com"), It.IsAny<SendOptions>()), Times.Once);
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
