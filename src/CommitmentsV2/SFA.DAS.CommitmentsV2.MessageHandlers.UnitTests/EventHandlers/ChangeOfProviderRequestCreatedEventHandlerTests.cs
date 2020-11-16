using AutoFixture;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.ProviderUrlHelper;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ChangeOfProviderRequestCreatedEventHandlerTests
    {

        private ChangeOfProviderRequestCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfProviderRequestCreatedEventHandlerTestsFixture();
        }

        [Test]
        public async Task WhenHandlingCommand_ThenProviderNameIsRetrieved()
        {
            await _fixture.ChangeOfProviderRequest().Handle();

            _fixture.VerifyGetProviderName();
        }

        [Test]
        public async Task WhenHandlingCommand_ThenRequestUrlIsGenerated()
        {
            await _fixture.ChangeOfProviderRequest().Handle();

            _fixture.VerifyRequestUrlIsGenerated();
        }

        [Test]
        public async Task WhenHandlingCommand_ThenEmailIsSentToProvider()
        {
            await _fixture.ChangeOfProviderRequest().Handle();

            _fixture.VerifyEmailSentToProvider();
        }


        public class ChangeOfProviderRequestCreatedEventHandlerTestsFixture
        {
            private readonly Fixture _autoFixture;
            private readonly GetProviderQueryResult _getProviderResponse;
            private const string _requestUrl = "https://pas.gov.uk/10000001/apprentices/ABC123/details";

            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IMessageHandlerContext> _mockMessageHandlerContext;
            private readonly Mock<IPipelineContext> _mockPipelineContext;
            private readonly Mock<ILinkGenerator> _mockLinkGenerator;

            private ChangeOfProviderRequestCreatedEvent _event;
            private readonly ChangeOfProviderRequestCreatedEventHandler _handler;

            public ChangeOfProviderRequestCreatedEventHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _getProviderResponse = _autoFixture.Create<GetProviderQueryResult>();

                _mockMessageHandlerContext = new Mock<IMessageHandlerContext>();
                _mockPipelineContext = _mockMessageHandlerContext.As<IPipelineContext>();

                _mockLinkGenerator = new Mock<ILinkGenerator>();
                _mockLinkGenerator.Setup(g => g.ProviderApprenticeshipServiceLink(It.IsAny<string>()))
                    .Returns(_requestUrl);

                _mediator = new Mock<IMediator>();
                _mediator.Setup(m => m.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_getProviderResponse);

                _handler = new ChangeOfProviderRequestCreatedEventHandler(_mediator.Object, _mockLinkGenerator.Object);
            }

            public ChangeOfProviderRequestCreatedEventHandlerTestsFixture ChangeOfProviderRequest()
            {
                _event = new ChangeOfProviderRequestCreatedEvent(_autoFixture.Create<string>(), _autoFixture.Create<string>(), _autoFixture.Create<long>(), _autoFixture.Create<string>());

                return this;
            }
            public async Task Handle()
            {
                await _handler.Handle(_event, _mockMessageHandlerContext.Object);
            }

            public void VerifyGetProviderName()
            {
                _mediator.Verify(m => m.Send(It.Is<GetProviderQuery>(q => q.ProviderId == _event.ProviderId), It.IsAny<CancellationToken>()), Times.Once);
            }

            public void VerifyRequestUrlIsGenerated()
            {
                _mockLinkGenerator.Verify(g => g.ProviderApprenticeshipServiceLink(It.IsAny<string>()), Times.Once);
            }

            public void VerifyEmailSentToProvider()
            {
                var apprenticeNamePossessive = _event.ApprenticeName.EndsWith("s") ? _event.ApprenticeName + "'" : _event.ApprenticeName + "'s";

                _mockPipelineContext.Verify(e => e.Send(It.Is<SendEmailToProviderCommand>(c =>
                    c.ProviderId == _event.ProviderId &&
                    c.Template == "" &&
                    c.Tokens["TrainingProviderName"] == _getProviderResponse.Name &&
                    c.Tokens["EmployerName"] == _event.EmployerName &&
                    c.Tokens["ApprenticeNamePossessive"] == apprenticeNamePossessive &&
                    c.Tokens["RequestUrl"] == _requestUrl &&
                    c.EmailAddress == null
                ), It.IsAny<SendOptions>()));
            }
        }
    }
}
