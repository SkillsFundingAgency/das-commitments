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

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    class OverlappingTrainingDateEventHandlerTests
    {
        private OverlappingTrainingDateEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateEventHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_ThenSendEmailToEmployerIsCalled()
        {
            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "OverlappingTrainingDate" &&
                    c.Tokens["EMPLOYERNAME"] == OverlappingTrainingDateEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["ULN"] == OverlappingTrainingDateEventHandlerTestsFixture.Uln &&
                    c.Tokens["APPRENTICENAME"] == $"{OverlappingTrainingDateEventHandlerTestsFixture.FirstName} {OverlappingTrainingDateEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["URL"] == $"{OverlappingTrainingDateEventHandlerTestsFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateEventHandlerTestsFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateEventHandlerTestsFixture.HashedApprenticeshipId}/details"
                    )
                  , It.IsAny<SendOptions>()), Times.Once);
        }
    }

    public class OverlappingTrainingDateEventHandlerTestsFixture : EventHandlerTestsFixture<OverlappingTrainingDateEvent, OverlappingTrainingDateEventHandler>
    {
        public Mock<ILogger<OverlappingTrainingDateEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }

        public OverlappingTrainingDateEvent Event { get; set; }

        private readonly Apprenticeship _apprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public readonly DateTime PausedDate;
        public const string FirstName = "TestFirst";
        public const string LastName = "TestLast";
        public const string Uln = "1234567899";
        public const string HashedEmployerAccountId = "1";
        public const string EmployerName = "TestEmployerName";
        public const string HashedApprenticeshipId = "ABC";
        public const string EmployerCommitmentsBaseUrl = "https://approvals/";

        public OverlappingTrainingDateEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<OverlappingTrainingDateEventHandler>>();

            var autoFixture = new Fixture();

            Event = autoFixture.Create<OverlappingTrainingDateEvent>();
            Event.Uln = Uln;

            var accountLegalEntity = new AccountLegalEntity();
            accountLegalEntity.SetValue(x => x.Name, EmployerName);

            _apprenticeship = new Apprenticeship();
            _apprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);

            _apprenticeship.SetValue(x => x.FirstName, FirstName);
            _apprenticeship.SetValue(x => x.LastName, LastName);
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

            Handler = new OverlappingTrainingDateEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { EmployerCommitmentsBaseUrl = EmployerCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
    }
}

