using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class CohortTransferApprovalRequestedEventHandlerTests
    {
        public CohortTransferApprovalRequestedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortTransferApprovalRequestedEventHandlerTestsFixture();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task When_HandlingCommand_TransferRequestIsCreated(Party lastApprovingParty)
        {
            await _fixture.WithLastApprovalParty(lastApprovingParty).Handle();
            _fixture.VerifyTransferRequestCreated();
        }

        [Test]
        public async Task When_HandlingCommand_IfLastApprovalWasProvider_EmailIsSentToEmployer()
        {
            await _fixture.WithLastApprovalParty(Party.Provider).Handle();
            _fixture.VerifyEmailSentToEmployer();
        }

        public class CohortTransferApprovalRequestedEventHandlerTestsFixture
        {
            private CohortTransferApprovalRequestedEventHandler _handler;
            private CohortTransferApprovalRequestedEvent _event;
            private Mock<IMediator> _mediator;
            private readonly Fixture _autoFixture;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            public Mock<IPipelineContext> _pipelineContext;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private readonly Mock<IEncodingService> _encodingService;
            private readonly string _cohortReference;
            private readonly string _employerEncodedAccountId;

            public CohortTransferApprovalRequestedEventHandlerTestsFixture()
            {
                
                _autoFixture = new Fixture();

                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _pipelineContext = _messageHandlerContext.As<IPipelineContext>();

                _cohortReference = _autoFixture.Create<string>();
                _employerEncodedAccountId = _autoFixture.Create<string>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.CohortId),
                    EncodingType.CohortReference)).Returns(_cohortReference);
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.AccountId),
                    EncodingType.AccountId)).Returns(_employerEncodedAccountId);

                _mediator = new Mock<IMediator>();
                _mediator.Setup(x => x.Send(It.IsAny<AddTransferRequestCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Unit());

                _cohortSummary = _autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_cohortSummary);

                _handler = new CohortTransferApprovalRequestedEventHandler(_mediator.Object, _encodingService.Object, Mock.Of<ILogger<CohortTransferApprovalRequestedEventHandler>>());

                _event = new CohortTransferApprovalRequestedEvent(_autoFixture.Create<long>(),
                    _autoFixture.Create<DateTime>(), _autoFixture.Create<Party>()
                    );
            }

            public CohortTransferApprovalRequestedEventHandlerTestsFixture WithLastApprovalParty(
                Party lastApprovalParty)
            {
                _event = new CohortTransferApprovalRequestedEvent(_autoFixture.Create<long>(),
                    _autoFixture.Create<DateTime>(), lastApprovalParty
                );

                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyTransferRequestCreated()
            {
                _mediator.Verify(x => x.Send(It.Is<AddTransferRequestCommand>(c => c.CohortId == _event.CohortId && c.LastApprovedByParty == _event.LastApprovedByParty),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }

            public void VerifyEmailSentToEmployer()
            {
                _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.AccountId == _cohortSummary.AccountId &&
                    c.EmailAddress == _cohortSummary.LastUpdatedByEmployerEmail &&
                    c.Template == "EmployerTransferPendingFinalApproval" &&
                    c.Tokens["provider_name"] == _cohortSummary.ProviderName &&
                    c.Tokens["sender_name"] == _cohortSummary.TransferSenderName &&
                    c.Tokens["employer_hashed_account"] == _employerEncodedAccountId &&
                    c.Tokens["cohort_reference"] == _cohortReference
                ), It.IsAny<SendOptions>()));
            }
        }
    }
}
