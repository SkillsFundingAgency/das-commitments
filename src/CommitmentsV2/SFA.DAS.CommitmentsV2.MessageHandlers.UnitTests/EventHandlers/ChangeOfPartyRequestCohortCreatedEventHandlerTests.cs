using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ChangeOfPartyRequestCohortCreatedEventHandlerTests
    {
        public ChangeOfPartyRequestCohortCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestCohortCreatedEventHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingEvent_IfLevyAccount_SendApproveNewEmployerDetails_Levy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.Levy).Handle();
            _fixture.VerifyEmailSent(ChangeOfPartyRequestCohortCreatedEventHandler.ApproveNewEmployerDetailsLevy);
        }

        [Test]
        public async Task When_HandlingEvent_IfLevyAccount_SendApproveNewEmployerDetails_NonLevy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.NonLevy).Handle();
            _fixture.VerifyEmailSent(ChangeOfPartyRequestCohortCreatedEventHandler.ApproveNewEmployerDetailsNonLevy);
        }

        public class ChangeOfPartyRequestCohortCreatedEventHandlerTestsFixture
        {
            private readonly ChangeOfPartyRequestCohortCreatedEventHandler _handler;
            private readonly ChangeOfPartyRequestCohortCreatedEvent _event;
            private readonly Mock<IMediator> _mediator;
            private readonly TestableMessageHandlerContext _messageHandlerContext;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private Fixture _autoFixture;

            public ChangeOfPartyRequestCohortCreatedEventHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                _mediator = new Mock<IMediator>();

                _cohortSummary = _autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _cohortSummary);

               _handler = new ChangeOfPartyRequestCohortCreatedEventHandler(_mediator.Object, 
                    Mock.Of<ILogger<ChangeOfPartyRequestCohortCreatedEventHandler>>());

                _messageHandlerContext = new TestableMessageHandlerContext();
                _event = _autoFixture.Create<ChangeOfPartyRequestCohortCreatedEvent>();
            }


            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext);
            }

            public ChangeOfPartyRequestCohortCreatedEventHandlerTestsFixture WithLevyStatus(ApprenticeshipEmployerType levyStatus)
            {
                _cohortSummary.LevyStatus = levyStatus;
                return this;
            }

            public void VerifyEmailSent(string templateName)
            {
               var emailToEmployerCommands =  _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToEmployerCommand)
                      .Select(y => y.Message as SendEmailToEmployerCommand);
                var emailToEmployerCommand = emailToEmployerCommands.First();

                Assert.AreEqual(1, emailToEmployerCommands.Count());
                Assert.AreEqual(_cohortSummary.AccountId, emailToEmployerCommand.AccountId);
                Assert.AreEqual(templateName, emailToEmployerCommand.Template);
                Assert.AreEqual(1, emailToEmployerCommand.Tokens.Count());
                Assert.AreEqual("provider_name", emailToEmployerCommand.Tokens.First().Key);
                Assert.AreEqual(_cohortSummary.ProviderName, emailToEmployerCommand.Tokens.First().Value);
            }
        }
    }
}
