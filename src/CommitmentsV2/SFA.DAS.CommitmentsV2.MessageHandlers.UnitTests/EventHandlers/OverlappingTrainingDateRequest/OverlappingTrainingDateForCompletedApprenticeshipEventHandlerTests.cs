using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    class OverlappingTrainingDateForCompletedApprenticeshipEventHandlerTests
    {
        private OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture();

        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStatus_Is_Completed_ThenSendEmailToEmployerIsCalled()
        {
            _fixture.WithPaymentStatus(PaymentStatus.Completed);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "EmployerOverlappingTrainingDateForCompletedApprenticeship" &&
                    c.NameToken == "Name" &&
                    c.Tokens["Uln"] == OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.FirstName} {OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.LastName}" &&
                    c.Tokens["EndDate"] == _fixture.EndDate.ToGdsFormatLongMonthWithoutDay() &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.HashedApprenticeshipId}/details"
                )
                , It.IsAny<SendOptions>()), Times.Once);
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn)]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStatus_Is_Not_Completed_ThenSendEmailToEmployerIsNotCalled(PaymentStatus paymentStatus)
        {
            _fixture.WithPaymentStatus(paymentStatus);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "EmployerOverlappingTrainingDateForCompletedApprenticeship" &&
                    c.NameToken == "Name" &&
                    c.Tokens["Uln"] == OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.FirstName} {OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.LastName}" &&
                    c.Tokens["EndDate"] == _fixture.EndDate.ToGdsFormatLongMonthWithoutDay() &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture.HashedApprenticeshipId}/details"
                    )
                  , It.IsAny<SendOptions>()), Times.Never);
        }
    }

    public class OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture : EventHandlerTestsFixture<OverlappingTrainingDateCreatedEvent, OverlappingTrainingDateForCompletedApprenticeshipEventHandler>
    {
        public Mock<ILogger<OverlappingTrainingDateForCompletedApprenticeshipEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }

        public OverlappingTrainingDateCreatedEvent Event { get; set; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string Uln = "1234567899";
        public const string HashedEmployerAccountId = "1";
        public const string EmployerName = "TestEmployerName";
        public readonly DateTime EndDate;
        public const string HashedApprenticeshipId = "ABC";
        public const string EmployerCommitmentsBaseUrl = "https://approvals/";

        public OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<OverlappingTrainingDateForCompletedApprenticeshipEventHandler>>();

            EndDate = DateTime.UtcNow;

            var autoFixture = new Fixture();

            Event = autoFixture.Create<OverlappingTrainingDateCreatedEvent>();
            Event.Uln = Uln;

            var accountLegalEntity = new AccountLegalEntity();
            accountLegalEntity.SetValue(x => x.Name, EmployerName);

            _apprenticeship = new Apprenticeship();
            _apprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);

            _apprenticeship.SetValue(x => x.FirstName, FirstName);
            _apprenticeship.SetValue(x => x.LastName, LastName);
            _apprenticeship.SetValue(x => x.EndDate, EndDate);
            _apprenticeship.SetValue(x => x.PaymentStatus, PaymentStatus.Completed);
            _apprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = accountLegalEntity,
                EmployerAccountId = 1
            });

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns(HashedEmployerAccountId);

            Handler = new OverlappingTrainingDateForCompletedApprenticeshipEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { EmployerCommitmentsBaseUrl = EmployerCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
        public OverlappingTrainingDateForCompletedApprenticeshipEventHandlerFixture WithPaymentStatus(PaymentStatus paymentStatus)
        {
            _apprenticeship.SetValue(x => x.PaymentStatus, paymentStatus);
            _db.SaveChanges();

            return this;
        }
    }
}

