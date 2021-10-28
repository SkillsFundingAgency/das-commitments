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
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
    {
        [TestFixture]
        public class TransferRequestRejectedEventHandlerForEmailNotificationsTests
    {
            public TransferRequestRejectedEventHandlerForEmailNotificationsTestsFixture _fixture;

            [SetUp]
            public void Arrange()
            {
                _fixture = new TransferRequestRejectedEventHandlerForEmailNotificationsTestsFixture();
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

            public class TransferRequestRejectedEventHandlerForEmailNotificationsTestsFixture
            {
                private TransferRequestRejectedEventHandlerForEmailNotifications _handler;
                private TransferRequestRejectedEvent _event;
                private Mock<IMediator> _mediator;
                private readonly Fixture _autoFixture;
                private Mock<IMessageHandlerContext> _messageHandlerContext;
                public Mock<IPipelineContext> _pipelineContext;
                private readonly GetCohortSummaryQueryResult _cohortSummary;
                private readonly Mock<IEncodingService> _encodingService;
                private readonly string _cohortReference;
                private readonly string _employerEncodedAccountId;
                private readonly long _providerId;
                private static CommitmentsV2Configuration commitmentsV2Configuration;

                public TransferRequestRejectedEventHandlerForEmailNotificationsTestsFixture()
                {
                    _autoFixture = new Fixture();

                    _messageHandlerContext = new Mock<IMessageHandlerContext>();
                    _pipelineContext = _messageHandlerContext.As<IPipelineContext>();

                    _providerId = _autoFixture.Create<long>();
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
                    _cohortSummary.ProviderId = _providerId;
                    _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_cohortSummary);

                    commitmentsV2Configuration = new CommitmentsV2Configuration()
                    {
                        ProviderCommitmentsBaseUrl = "https://approvals.environmentname-pas.apprenticeships.education.gov.uk/"
                    };

                     _handler = new TransferRequestRejectedEventHandlerForEmailNotifications(_mediator.Object, _encodingService.Object, commitmentsV2Configuration);

                    _event = new TransferRequestRejectedEvent(_autoFixture.Create<long>(),
                        _autoFixture.Create<long>(),
                        _autoFixture.Create<DateTime>(),
                        _autoFixture.Create<UserInfo>()
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
                        c.Template == "SenderRejectedCommitmentEmployerNotification" &&
                        c.Tokens["cohort_reference"] == _cohortReference &&
                        c.Tokens["employer_name"] == _cohortSummary.LegalEntityName &&
                        c.Tokens["sender_name"] == _cohortSummary.TransferSenderName &&
                        c.Tokens["employer_hashed_account"] == _employerEncodedAccountId
                    ), It.IsAny<SendOptions>()));
                }

                public void VerifyEmailSentToProvider()
                {
                    _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToProviderCommand>(c =>
                        c.EmailAddress == _cohortSummary.LastUpdatedByProviderEmail &&
                        c.Template == "SenderRejectedCommitmentProviderNotification" &&
                        c.Tokens["cohort_reference"] == _cohortReference &&
                        c.Tokens["RequestUrl"] == $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{_providerId}/unapproved/{_cohortReference}/details"
                    ), It.IsAny<SendOptions>()));
                }
            }
        }
    }
