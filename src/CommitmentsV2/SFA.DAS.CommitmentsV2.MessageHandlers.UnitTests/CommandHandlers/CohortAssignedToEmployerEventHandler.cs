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
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
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
        public async Task When_HandlingEvent_SendEmailToEmployer()
        {
            await _fixture.Handle();
            _fixture.VerifyEmailSent();
        }

        public class CohortAssignedToEmployerEventHandlerTestsFixture
        {
            private readonly CohortAssignedToEmployerEventHandler _handler;
            private readonly CohortAssignedToEmployerEvent _event;
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            private readonly Mock<IEventPublisher> _eventPublisher;
            private readonly Mock<IEncodingService> _encodingService;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private readonly string _cohortReference;

            public CohortAssignedToEmployerEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _mediator = new Mock<IMediator>();

                _cohortSummary = autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_cohortSummary);

                _cohortReference = autoFixture.Create<string>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.CohortId),
                        EncodingType.CohortReference)).Returns(_cohortReference);

                _eventPublisher = new Mock<IEventPublisher>();
                _eventPublisher.Setup(x => x.Publish(It.IsAny<SendEmailToEmployerCommand>()));

                _handler = new CohortAssignedToEmployerEventHandler(_mediator.Object, _eventPublisher.Object, _encodingService.Object);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<CohortAssignedToEmployerEvent>();

            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyEmailSent()
            {
                _eventPublisher.Verify(x => x.Publish(It.Is<SendEmailToEmployerCommand>(c =>
                    c.AccountId == _cohortSummary.AccountId &&
                    c.EmailAddress == _cohortSummary.LastUpdatedByEmployerEmail &&
                    c.Template == "EmployerCohortNotification" &&
                    c.Tokens["cohort_reference"] == _cohortReference &&
                    c.Tokens["type"] == (_cohortSummary.IsApprovedByProvider ? "approval" : "review")
                    )));
            }
        }
    }
}
