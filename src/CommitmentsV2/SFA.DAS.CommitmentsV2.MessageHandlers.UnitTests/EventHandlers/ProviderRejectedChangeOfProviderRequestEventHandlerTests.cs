using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;

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
        public async Task WhenHandlingEvent_AndChangeOfPartyTypeIsChangeOfProvider_ThenSendEmailCommandShouldBeSent()
        {
            _fixture.SetChangeOfPartyRequestTypeToChangeProvider();

            await _fixture.Handle();

            _fixture.VerifyConfirmationEmailSentToEmployer();
        }

        [Test]
        public async Task WhenHandlingEvent_AndChangeOfPartyTypeIsChangeOfEmployer_ThenNoEmailShouldBeSent()
        {
            _fixture.SetChangeOfPartyRequestTypeToChangeEmployer();

            await _fixture.Handle();

            _fixture.VerifyNoEmailIsSentToEmployer();
        }

    }

    class ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture
    {
        private const string AccountHashedId = "ABC123";
        private const long ApprenticeshipId = 123456;
        private const string ApprenticeshipHashedId = "XYX789";

        public Mock<IEncodingService> _mockEncodingService { get; set; }
        public IFixture _autoFixture { get; set; }
        public ProviderRejectedChangeOfPartyRequestEventHandler _handler { get; set; }
        private Mock<ChangeOfPartyRequest> _changeOfPartyRequest { get; set; }
        private Mock<IMessageHandlerContext> _mockMessageHandlerContext { get; set; }
        public Mock<IPipelineContext> _mockPipelineContext { get; set; }

        private readonly Mock<ProviderCommitmentsDbContext> _db;
        private readonly Cohort _cohort;
        public ProviderRejectedChangeOfPartyRequestEvent _event { get; set; }

        public ProviderRejectedChangeOfProviderRequestEventHandlerTestFixture()
        {
            _autoFixture = new Fixture();

            _mockMessageHandlerContext = new Mock<IMessageHandlerContext>();
            _mockPipelineContext = _mockMessageHandlerContext.As<IPipelineContext>();

            _event = _autoFixture.Create<ProviderRejectedChangeOfPartyRequestEvent>();

            _mockEncodingService = new Mock<IEncodingService>();
            _mockEncodingService.Setup(enc => enc.Encode(_event.EmployerAccountId, EncodingType.AccountId))
                .Returns(AccountHashedId);
            _mockEncodingService.Setup(enc => enc.Encode(ApprenticeshipId, EncodingType.ApprenticeshipId))
                .Returns(ApprenticeshipHashedId);

            _cohort = new Cohort();
            _cohort.SetValue(x => x.ChangeOfPartyRequestId, _event.ChangeOfPartyRequestId);

            _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
            _changeOfPartyRequest.Setup(x => x.Id).Returns(_event.ChangeOfPartyRequestId);
            _changeOfPartyRequest.Setup(x => x.ApprenticeshipId).Returns(ApprenticeshipId);

            _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };

            _db
                .Setup(context => context.Cohorts)
                .ReturnsDbSet(new List<Cohort> { _cohort });

            _db
                .Setup(context => context.ChangeOfPartyRequests)
                .ReturnsDbSet(new List<ChangeOfPartyRequest> { _changeOfPartyRequest.Object });


            _handler = new ProviderRejectedChangeOfPartyRequestEventHandler(_mockEncodingService.Object, new Lazy<ProviderCommitmentsDbContext>(() => _db.Object));
        }
        public Task Handle()
        {
            return _handler.Handle(_event, _mockMessageHandlerContext.Object);
        }

        public void SetChangeOfPartyRequestTypeToChangeProvider()
        {
            _changeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeProvider);
        }

        public void SetChangeOfPartyRequestTypeToChangeEmployer()
        {
            _changeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeEmployer);
        }

        public void VerifyConfirmationEmailSentToEmployer()
        {
            var apprenticeNamePossessive = _event.ApprenticeName.EndsWith("s") ? _event.ApprenticeName + "'" : _event.ApprenticeName + "'s";

            _mockPipelineContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(e =>
               e.AccountId == _event.EmployerAccountId &&
               e.Template == "TrainingProviderRejectedChangeOfProviderCohort" &&
               e.Tokens["EmployerName"] == _event.EmployerName &&
               e.Tokens["TrainingProviderName"] == _event.TrainingProviderName &&
               e.Tokens["ApprenticeNamePossessive"] == apprenticeNamePossessive &&
               e.Tokens["AccountHashedId"] == AccountHashedId &&
               e.Tokens["ApprenticeshipHashedId"] == ApprenticeshipHashedId &&
               e.EmailAddress == _event.RecipientEmailAddress
            ), It.IsAny<SendOptions>()));
        }

        public void VerifyNoEmailIsSentToEmployer()
        {
            _mockPipelineContext.Verify(x => x.Publish(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<PublishOptions>()), Times.Never);
        }
    }
}
