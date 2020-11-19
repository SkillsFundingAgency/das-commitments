using AutoFixture;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ProviderRejectedChangeOfProviderRequestEventHandlerTests
    {
        private ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture();
        }

        [Test]
        public async Task WhenHandlingEvent_ThenSendEmailCommandShouldBeSent()
        {
            await _fixture.Handle();

            _fixture.VerifyEmailSentToEmployer();
        }

        [Test]
        public async Task WhenHandlingEvent_ThenGetChangeOfPartyRequestIsCalled()
        {
            await _fixture.Handle();

            _fixture.VerifyChangeOfPartyRequestIsCalled();
        }
    }

    class ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture
    {
        private const string AccountHashedId = "ABC123";
        private const string ApprenticeshipHashedId = "XYX789";

        public Mock<IEncodingService> MockEncodingService { get; set; }
        public Mock<IMediator> MockMediator { get; set; }
        public IFixture AutoFixture { get; set; }
        public ProviderRejectedChangeOfProviderRequestEventHandler Handler { get; set; }
        private GetChangeOfPartyRequestQueryResult ChangeOfPartyRequest { get; set; }
        private Mock<IMessageHandlerContext> MockMessageHandlerContext { get; set; }
        public Mock<IPipelineContext> MockPipelineContext { get; set; }

        public ProviderRejectedChangeOfProviderRequestEvent Event { get; set; }

        public ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture()
        {
            AutoFixture = new Fixture();

            MockMessageHandlerContext = new Mock<IMessageHandlerContext>();
            MockPipelineContext = MockMessageHandlerContext.As<IPipelineContext>();

            Event = AutoFixture.Create<ProviderRejectedChangeOfProviderRequestEvent>();

            MockMediator = new Mock<IMediator>();
            ChangeOfPartyRequest = AutoFixture.Create<GetChangeOfPartyRequestQueryResult>();
            MockMediator.Setup(m => m.Send(It.Is<GetChangeOfPartyRequestQuery>(r => r.ChangeOfPartyRequestId == Event.ChangeOfPartyRequestId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ChangeOfPartyRequest);

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(enc => enc.Encode(Event.EmployerAccountId, EncodingType.AccountId))
                .Returns(AccountHashedId);
            MockEncodingService.Setup(enc => enc.Encode(ChangeOfPartyRequest.ApprenticeshipId, EncodingType.ApprenticeshipId))
                .Returns(ApprenticeshipHashedId);
            Handler = new ProviderRejectedChangeOfProviderRequestEventHandler(MockEncodingService.Object, MockMediator.Object);
            
        }
        public Task Handle()
        {
            return Handler.Handle(Event, MockMessageHandlerContext.Object);
        }

        public void VerifyEmailSentToEmployer()
        {
            var apprenticeNamePossessive = Event.ApprenticeName.EndsWith("s") ? Event.ApprenticeName + "'" : Event.ApprenticeName + "'s";
            
            MockPipelineContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(e =>
               e.AccountId == Event.EmployerAccountId &&
               e.Template == "TrainingProviderRejectedChangeOfProviderCohort" &&
               e.Tokens["EmployerName"] == Event.EmployerName &&
               e.Tokens["TrainingProviderName"] == Event.TrainingProviderName &&
               e.Tokens["ApprenticeNamePossessive"] == apprenticeNamePossessive &&
               e.Tokens["AccountHashedId"] == AccountHashedId &&
               e.Tokens["ApprenticeshipHashedId"] == ApprenticeshipHashedId
            ), It.IsAny<SendOptions>()));
        }

        public void VerifyChangeOfPartyRequestIsCalled()
        {
            MockMediator.Verify(m => m.Send(It.Is<GetChangeOfPartyRequestQuery>(q => q.ChangeOfPartyRequestId == Event.ChangeOfPartyRequestId), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
