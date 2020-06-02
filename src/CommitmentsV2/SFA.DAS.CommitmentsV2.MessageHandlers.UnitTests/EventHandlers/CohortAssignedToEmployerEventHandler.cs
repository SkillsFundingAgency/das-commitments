using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
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
    public class CohortAssignedToEmployerEventHandlerTests
    {
        public CohortAssignedToEmployerEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortAssignedToEmployerEventHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingEvent_IfAssignedByProvider_AndNotChangeOfParty_SendEmailToEmployer()
        {
            await _fixture.WithAssignmentByParty(Party.Provider).SetChangeOfPartyRequestId(null).Handle();
            _fixture.VerifyEmailSent();
        }

        [Test]
        public async Task When_HandlingEvent_IfAssignedByProvider_AndIsChangeOfParty_EmailIsNotSent()
        {
            await _fixture.WithAssignmentByParty(Party.Provider).SetChangeOfPartyRequestId(1).Handle();
            _fixture.VerifyEmailNotSent();
        }

        [Test]
        public async Task When_HandlingEvent_IfAssignedByTransferSender_AndNotChangeOfParty_EmailIsNotSent()
        {
            await _fixture.WithAssignmentByParty(Party.TransferSender).SetChangeOfPartyRequestId(null).Handle();
            _fixture.VerifyEmailNotSent();
        }

        public class CohortAssignedToEmployerEventHandlerTestsFixture
        {
            private readonly CohortAssignedToEmployerEventHandler _handler;
            private CohortAssignedToEmployerEvent _event;
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            public Mock<IPipelineContext> _pipelineContext;
            private readonly Mock<IEncodingService> _encodingService;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private readonly string _cohortReference;
            private readonly string _employerEncodedAccountId;
            private Fixture _autoFixture;

            public CohortAssignedToEmployerEventHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                _mediator = new Mock<IMediator>();

                _cohortSummary = _autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _cohortSummary);

                _cohortReference = _autoFixture.Create<string>();
                _employerEncodedAccountId = _autoFixture.Create<string>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.CohortId),
                        EncodingType.CohortReference)).Returns(_cohortReference);
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.AccountId),
                    EncodingType.AccountId)).Returns(_employerEncodedAccountId);

                _handler = new CohortAssignedToEmployerEventHandler(_mediator.Object, _encodingService.Object);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _pipelineContext = _messageHandlerContext.As<IPipelineContext>();

                _event = _autoFixture.Create<CohortAssignedToEmployerEvent>();
            }

            public CohortAssignedToEmployerEventHandlerTestsFixture WithAssignmentByParty(Party assigningParty)
            {
                _event = new CohortAssignedToEmployerEvent(_autoFixture.Create<long>(),
                    _autoFixture.Create<DateTime>(),
                    assigningParty);
                return this;
            }

            public CohortAssignedToEmployerEventHandlerTestsFixture SetChangeOfPartyRequestId(long? changeOfPartyRequestId)
            {
                _cohortSummary.ChangeOfPartyRequestId = changeOfPartyRequestId;
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyEmailSent()
            {
                _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.AccountId == _cohortSummary.AccountId &&
                    c.EmailAddress == _cohortSummary.LastUpdatedByEmployerEmail &&
                    c.Template == "EmployerCohortNotification" &&
                    c.Tokens["provider_name"] == _cohortSummary.ProviderName &&
                    c.Tokens["employer_hashed_account"] == _employerEncodedAccountId &&
                    c.Tokens["cohort_reference"] == _cohortReference
                    ), It.IsAny<SendOptions>()));
            }

            public void VerifyEmailNotSent()
            {
                _pipelineContext.Verify(x => x.Send(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<SendOptions>()), Times.Never());
            }
        }
    }
}
