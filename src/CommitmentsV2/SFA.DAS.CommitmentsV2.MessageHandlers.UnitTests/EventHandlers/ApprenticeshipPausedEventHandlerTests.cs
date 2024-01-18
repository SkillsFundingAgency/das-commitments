using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipPausedEventHandlerTests
    {
        private ApprenticeshipPausedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipPausedEventHandlerTestsFixture();
        }

        [TearDown]
        public void TearDown() => _fixture.Dispose();

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenEncodingServiceIsCalled()
        {
            _fixture.SetApprenticeshipStatus(PaymentStatus.Paused);
            
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_And_PaymentStatus_IsPaused_ThenSendEmailToProviderIsCalled()
        {
            _fixture.SetApprenticeshipStatus(PaymentStatus.Paused);
            
            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(c =>
                    c.Template == "ProviderApprenticeshipPauseNotification" &&
                    c.Tokens["EMPLOYER"] == ApprenticeshipPausedEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["APPRENTICE"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.FirstName} {ApprenticeshipPausedEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["DATE"] == _fixture.PausedDate.ToString("dd/MM/yyyy") &&
                    c.Tokens["URL"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}/1/apprentices/{ApprenticeshipPausedEventHandlerTestsFixture.HashedApprenticeshipId}"
                    )
                  , It.IsAny<SendOptions>()), Times.Once);
        }
        
        [Test]
        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Withdrawn)]
        public async Task WhenHandlingApprenticeshipPauseEvent_And_PaymentStatus_Is_Not_Paused_ThenSendEmailToProviderIsNotCalled(PaymentStatus status)
        {
            _fixture.SetApprenticeshipStatus(status);
            
            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(c =>
                    c.Template == "ProviderApprenticeshipPauseNotification" &&
                    c.Tokens["EMPLOYER"] == ApprenticeshipPausedEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["APPRENTICE"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.FirstName} {ApprenticeshipPausedEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["DATE"] == _fixture.PausedDate.ToString("dd/MM/yyyy") &&
                    c.Tokens["URL"] == $"{ApprenticeshipPausedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}/1/apprentices/{ApprenticeshipPausedEventHandlerTestsFixture.HashedApprenticeshipId}"
                )
                , It.IsAny<SendOptions>()), Times.Never);
        }
    }

    public class ApprenticeshipPausedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipPausedEvent, ApprenticeshipPausedEventHandler>
    {
        public Mock<ILogger<ApprenticeshipPausedEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }

        public ApprenticeshipPausedEvent Event { get; set; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public readonly DateTime PausedDate;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string EmployerName = "TestEmployerName";
        public const string HashedApprenticeshipId = "ABC";
        public const string ProviderCommitmentsBaseUrl = "https://approvals/";

        public ApprenticeshipPausedEventHandlerTestsFixture() : base((m) => null)
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

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);

            Handler = new ApprenticeshipPausedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl });
        }

        public void SetApprenticeshipStatus(PaymentStatus status)
        {
            _apprenticeship.PaymentStatus = status;
        }

        public void Dispose() => _db?.Dispose();
        
        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
    }
}