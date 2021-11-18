using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
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

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
    {
        [TestFixture]
        public class TransferRequestApprovedEventHandlerForEmailNotificationsTests
        {
            public TransferRequestApprovedEventHandlerForEmailNotificationsTestsFixture _fixture;

            [SetUp]
            public void Arrange()
            {
                _fixture = new TransferRequestApprovedEventHandlerForEmailNotificationsTestsFixture();
            }

            [Test]
            public async Task When_HandlingCommand_EmailIsSentToProvider()
            {
                await _fixture.Handle();
                _fixture.VerifyEmailSentToProvider();
            }

            [Test]
            public async Task When_HandlingCommand_EmailIsSentToEmployer()
            {
                await _fixture.Handle();
                _fixture.VerifyEmailSentToEmployer();
            }

        public class TransferRequestApprovedEventHandlerForEmailNotificationsTestsFixture
        {
                private TransferRequestApprovedEventHandlerForEmailNotifications _handler;
                private TransferRequestApprovedEvent _event;
                private Mock<IMediator> _mediator;
                private readonly Fixture _autoFixture;
                private Mock<IMessageHandlerContext> _messageHandlerContext;
                public Mock<IPipelineContext> _pipelineContext;
                private readonly GetCohortSummaryQueryResult _cohortSummary;
                private readonly Mock<IEncodingService> _encodingService;
                private readonly string _cohortReference;
                private readonly string _employerEncodedAccountId;

                public TransferRequestApprovedEventHandlerForEmailNotificationsTestsFixture()
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

                    _handler = new TransferRequestApprovedEventHandlerForEmailNotifications(_mediator.Object, _encodingService.Object);

                    _event = new TransferRequestApprovedEvent(_autoFixture.Create<long>(),
                        _autoFixture.Create<long>(),
                        _autoFixture.Create<DateTime>(),
                        _autoFixture.Create<UserInfo>(),
                        _autoFixture.Create<int>(),
                        _autoFixture.Create<decimal>(),
                        _autoFixture.Create<int?>()
                        );
                }

                public async Task Handle()
                {
                    await _handler.Handle(_event, _messageHandlerContext.Object);
                }

                public void VerifyEmailSentToEmployer()
                {
                    _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
                        c.EmailAddress == _cohortSummary.LastUpdatedByEmployerEmail &&
                        c.Template == "SenderApprovedCommitmentEmployerNotification" &&
                        c.Tokens["cohort_reference"] == _cohortReference &&
                        c.Tokens["employer_name"] == _cohortSummary.LegalEntityName &&
                        c.Tokens["sender_name"] == _cohortSummary.TransferSenderName
                    ), It.IsAny<SendOptions>()));
                }

                public void VerifyEmailSentToProvider()
                {
                    _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToProviderCommand>(c =>
                        c.EmailAddress == _cohortSummary.LastUpdatedByProviderEmail &&
                        c.Template == "SenderApprovedCommitmentProviderNotification" &&
                        c.Tokens["cohort_reference"] == _cohortReference
                    ), It.IsAny<SendOptions>()));
                }
        }
     }
}