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
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    class OverlappingTrainingDateEditEndDateEventHandlerTests
    {
        private OverlappingTrainingDateEditEndDateEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateEditEndDateEventHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.MockEncodingService.Verify(x => x.Encode(_fixture.Event.ApprenticeshipId, EncodingType.ApprenticeshipId), Times.Once);
        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStaus_Is_Completed_ThenSendEmailToEmployerIsCalled()
        {
            _fixture.WithApprenticeshipStatus(PaymentStatus.Completed);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "OverlappingTrainingDateEditEndDate" &&
                    c.Tokens["FirstName"] == OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["Uln"] == OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.FirstName} {OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["EndDate"] == _fixture.EndDate.ToString("dd/MM/yyyy") &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.HashedApprenticeshipId}/details"
                )
                , It.IsAny<SendOptions>()), Times.Once);
        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateEvent_If_PaymentStaus_Is_Not_Completed_ThenSendEmailToEmployerIsNotCalled()
        {
            _fixture.WithApprenticeshipStatus(PaymentStatus.Active);

            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(c =>
                    c.Template == "OverlappingTrainingDateEditEndDate" &&
                    c.Tokens["FirstName"] == OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.EmployerName &&
                    c.Tokens["Uln"] == OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.Uln &&
                    c.Tokens["Apprentice"] == $"{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.FirstName} {OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.LastName}" &&
                    c.Tokens["EndDate"] == _fixture.EndDate.ToString("dd/MM/yyyy") &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.EmployerCommitmentsBaseUrl}/{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.HashedEmployerAccountId}/apprentices/{OverlappingTrainingDateEditEndDateEventHandlerTestsFixture.HashedApprenticeshipId}/details"
                    )
                  , It.IsAny<SendOptions>()), Times.Never);
        }
    }

    public class OverlappingTrainingDateEditEndDateEventHandlerTestsFixture : EventHandlerTestsFixture<OverlappingTrainingDateEvent, OverlappingTrainingDateEditEndDateEventHandler>
    {
        public Mock<ILogger<OverlappingTrainingDateEditEndDateEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }

        public OverlappingTrainingDateEvent Event { get; set; }

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

        public OverlappingTrainingDateEditEndDateEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<OverlappingTrainingDateEditEndDateEventHandler>>();

            EndDate = DateTime.UtcNow;

            var autoFixture = new Fixture();

            Event = autoFixture.Create<OverlappingTrainingDateEvent>();
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _db.Apprenticeships.Add(_apprenticeship);
            _db.SaveChanges();

            MockEncodingService = new Mock<IEncodingService>();
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(HashedApprenticeshipId);
            MockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns(HashedEmployerAccountId);

            Handler = new OverlappingTrainingDateEditEndDateEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object, MockEncodingService.Object,
                new CommitmentsV2Configuration { EmployerCommitmentsBaseUrl = EmployerCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
        public OverlappingTrainingDateEditEndDateEventHandlerTestsFixture WithApprenticeshipStatus(PaymentStatus paymentStatus)
        {
            _apprenticeship.SetValue(x => x.PaymentStatus, paymentStatus);
            _db.SaveChanges();

            return this;
        }
    }
}

