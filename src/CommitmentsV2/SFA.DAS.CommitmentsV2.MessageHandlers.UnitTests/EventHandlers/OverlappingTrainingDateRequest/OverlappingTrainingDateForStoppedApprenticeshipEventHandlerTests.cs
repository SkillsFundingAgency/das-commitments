using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Events.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    class OverlappingTrainingDateForStoppedApprenticeshipEventHandlerTests
    {
        private OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture();

        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStatus_Is_Stopped_ThenSendEmailToEmployerIsCalled()
        {
            _fixture.WithPaymentStatus(PaymentStatus.Withdrawn);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "EmployerOverlappingTrainingDateForStoppedApprenticeship" &&
                    c.NameToken == "Name" &&
                    c.Tokens["Uln"] == OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.FirstName} {OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.LastName}" &&
                    c.Tokens["StopDate"] == _fixture.StopDate.ToGdsFormatLongMonthWithoutDay() &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.HashedApprenticeshipId}/details"
                )
                , It.IsAny<SendOptions>()), Times.Once);
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Completed)]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStatus_Is_Not_Stopped_ThenSendEmailToEmployerIsNotCalled(PaymentStatus paymentStatus)
        {
            _fixture.WithPaymentStatus(paymentStatus);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "EmployerOverlappingTrainingDateForStoppedApprenticeship" &&
                    c.NameToken == "Name" &&
                    c.Tokens["Uln"] == OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.FirstName} {OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.LastName}" &&
                    c.Tokens["StopDate"] == _fixture.StopDate.ToGdsFormatLongMonthWithoutDay() &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture.HashedApprenticeshipId}/details"
                    )
                  , It.IsAny<SendOptions>()), Times.Never);
        }
    }

    public class OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture : EventHandlerTestsFixture<OverlappingTrainingDateCreatedEvent, OverlappingTrainingDateForStoppedApprenticeshipEventHandler>
    {
        public Mock<ILogger<OverlappingTrainingDateForStoppedApprenticeshipEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }

        public OverlappingTrainingDateCreatedEvent Event { get; set; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string Uln = "1234567899";
        public const string HashedEmployerAccountId = "1";
        public const string EmployerName = "TestEmployerName";
        public readonly DateTime StopDate;
        public const string HashedApprenticeshipId = "ABC";
        public const string EmployerCommitmentsBaseUrl = "https://approvals/";

        public OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<OverlappingTrainingDateForStoppedApprenticeshipEventHandler>>();

            StopDate = DateTime.UtcNow;

            var autoFixture = new Fixture();

            Event = autoFixture.Create<OverlappingTrainingDateCreatedEvent>();
            Event.Uln = Uln;

            var accountLegalEntity = new AccountLegalEntity();
            accountLegalEntity.SetValue(x => x.Name, EmployerName);

            _apprenticeship = new Apprenticeship();
            _apprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);

            _apprenticeship.SetValue(x => x.FirstName, FirstName);
            _apprenticeship.SetValue(x => x.LastName, LastName);
            _apprenticeship.SetValue(x => x.StopDate, StopDate);
            _apprenticeship.SetValue(x => x.PaymentStatus, PaymentStatus.Withdrawn);
            _apprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = accountLegalEntity,
                EmployerAccountId = 1
            });

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns(HashedEmployerAccountId);

            Handler = new OverlappingTrainingDateForStoppedApprenticeshipEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { EmployerCommitmentsBaseUrl = EmployerCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
        public OverlappingTrainingDateForStoppedApprenticeshipEventHandlerFixture WithPaymentStatus(PaymentStatus paymentStatus)
        {
            _apprenticeship.SetValue(x => x.PaymentStatus, paymentStatus);
            _db.SaveChanges();

            return this;
        }
    }
}

