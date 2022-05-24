using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.Encoding;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipResumedEventHandlerTests
    {
        private ApprenticeshipResumedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipResumedEventHandlerTestsFixture();
        }

        [Test]
        public async Task WhenHandlingApprenticeshipResumeEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingApprenticeshipResumeEvent_ThenSendEmailToProviderIsCalled()
        {
            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(c =>
                    c.Template == "ProviderApprenticeshipResumeNotification" &&
                    c.Tokens["EMPLOYER"] == ApprenticeshipResumedEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["APPRENTICE"] == $"{ApprenticeshipResumedEventHandlerTestsFixture.FirstName} {ApprenticeshipResumedEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["DATE"] == _fixture.Event.ResumedOn.ToString("dd/MM/yyyy") &&
                    c.Tokens["URL"] == $"{ApprenticeshipResumedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}/{ApprenticeshipResumedEventHandlerTestsFixture.ProviderId}/apprentices/{ApprenticeshipResumedEventHandlerTestsFixture.HashedApprenticeshipId}"
                    )
                  , It.IsAny<SendOptions>()), Times.Once);
        }
    }

    public class ApprenticeshipResumedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipResumedEvent, ApprenticeshipResumedEventHandler>
    {
        public Mock<ILogger<ApprenticeshipResumedEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }
        public ApprenticeshipResumedEvent Event { get; set; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public readonly DateTime ResumedDate;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string EmployerName = "TestEmployerName";
        public const string HashedApprenticeshipId = "ABC";
        public const long ProviderId = 1;
        public const string ProviderCommitmentsBaseUrl = "https://approvals/";

        public ApprenticeshipResumedEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<ApprenticeshipResumedEventHandler>>();
            ResumedDate = DateTime.UtcNow;

            var autoFixture = new Fixture();

            Event = autoFixture.Create<ApprenticeshipResumedEvent>();
            var accountLegalEntity = new AccountLegalEntity();
            accountLegalEntity.SetValue(x => x.Name, EmployerName);

            _apprenticeship = new Apprenticeship();
            _apprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);

            _apprenticeship.SetValue(x => x.FirstName, FirstName);
            _apprenticeship.SetValue(x => x.LastName, LastName);
            _apprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = accountLegalEntity,
                ProviderId = ProviderId
            });

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);

            Handler = new ApprenticeshipResumedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), MockEncodingService.Object, 
                Logger.Object, new CommitmentsV2Configuration { ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
    }
}
