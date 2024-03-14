using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipPausedEventHandlerTests
    {
        private ApprenticeshipPausedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange() => _fixture = new ApprenticeshipPausedEventHandlerTestsFixture();

        [TearDown]
        public void TearDown() => _fixture.Dispose();

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();

            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(GetAllPaymentStatus))]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenSendEmailToProviderIsCalled_OnlyWhen_PaymentStatus_Is_Paused(PaymentStatus status)
        {
            _fixture.SetPaymentStatus(status);

            await _fixture.Handle();
            
            if (status == PaymentStatus.Paused)
            {
                _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(command =>
                    command.Template == ApprenticeshipPausedEventHandler.EmailTemplateName &&
                    command.Tokens["EMPLOYER"] == ApprenticeshipPausedEventHandlerTestsFixture.EmployerName &&
                    command.Tokens["APPRENTICE"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.FirstName} {ApprenticeshipPausedEventHandlerTestsFixture.LastName}" &&
                    command.Tokens["DATE"] == _fixture.PausedDate.ToString("dd/MM/yyyy") &&
                    command.Tokens["URL"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}1/apprentices/{ApprenticeshipPausedEventHandlerTestsFixture.HashedApprenticeshipId}"
                ), It.IsAny<SendOptions>()), Times.Once);
            }
            else
            {
                _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(command =>
                    command.Template == ApprenticeshipPausedEventHandler.EmailTemplateName &&
                    command.Tokens["EMPLOYER"] == ApprenticeshipPausedEventHandlerTestsFixture.EmployerName &&
                    command.Tokens["APPRENTICE"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.FirstName} {ApprenticeshipPausedEventHandlerTestsFixture.LastName}" &&
                    command.Tokens["DATE"] == _fixture.PausedDate.ToString("dd/MM/yyyy") &&
                    command.Tokens["URL"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}1/apprentices/{ApprenticeshipPausedEventHandlerTestsFixture.HashedApprenticeshipId}"
                ), It.IsAny<SendOptions>()), Times.Never);
            }
        }

        private static List<PaymentStatus> GetAllPaymentStatus() => Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>().ToList();
    }

    public class ApprenticeshipPausedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipPausedEvent, ApprenticeshipPausedEventHandler>
    {
        public Mock<ILogger<ApprenticeshipPausedEventHandler>> Logger { get; }
        public Mock<IEncodingService> MockEncodingService { get; }
        public ApprenticeshipPausedEvent Event { get; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public readonly DateTime PausedDate;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string EmployerName = "TestEmployerName";
        public const string HashedApprenticeshipId = "ABC";
        public const string ProviderCommitmentsBaseUrl = "https://approvals/";

        public ApprenticeshipPausedEventHandlerTestsFixture() : base(m => null)
        {
            Logger = new Mock<ILogger<ApprenticeshipPausedEventHandler>>();
            PausedDate = DateTime.UtcNow;

            var autoFixture = new Fixture();

            Event = autoFixture.Create<ApprenticeshipPausedEvent>();
            var accountLegalEntity = new AccountLegalEntity();
            accountLegalEntity.SetValue(x => x.Name, EmployerName);

            _apprenticeship = new Apprenticeship();
            _apprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);

            _apprenticeship.SetValue(x => x.FirstName, FirstName);
            _apprenticeship.SetValue(x => x.LastName, LastName);
            _apprenticeship.SetValue(x => x.PauseDate, PausedDate);
            _apprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = accountLegalEntity,
                ProviderId = 1
            });

            _apprenticeship.PaymentStatus = PaymentStatus.Paused;

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);

            Handler = new ApprenticeshipPausedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl });
        }

        public void SetPaymentStatus(PaymentStatus status) => _apprenticeship.PaymentStatus = status;

        public void Dispose() => _db?.Dispose();

        public override Task Handle() => Handler.Handle(Event, MessageHandlerContext.Object);
    }
}