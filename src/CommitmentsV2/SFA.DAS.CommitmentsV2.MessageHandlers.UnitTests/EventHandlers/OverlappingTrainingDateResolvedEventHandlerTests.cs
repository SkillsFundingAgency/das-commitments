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
    class OverlappingTrainingDateResolvedEventHandlerTests
    {
        private OverlappingTrainingDateResolvedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateResolvedEventHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingOverlappingTrainingDateResolvedEvent_ThenSendEmailToProviderIsCalled()
        {
            await _fixture.Handle();

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(c =>
                    c.Template == "OverlappingTrainingDateResolved" &&
                    c.Tokens["ProviderName"] == OverlappingTrainingDateResolvedEventHandlerTestsFixture.ProviderName &&
                    c.Tokens["CohortReference"] == OverlappingTrainingDateResolvedEventHandlerTestsFixture.CohortReference &&
                    c.EmailAddress == OverlappingTrainingDateResolvedEventHandlerTestsFixture.Email &&
                    c.Tokens["Url"] == $"{OverlappingTrainingDateResolvedEventHandlerTestsFixture.ProviderCommitmentsBaseUrl}{OverlappingTrainingDateResolvedEventHandlerTestsFixture.ProviderId}/unapproved/{OverlappingTrainingDateResolvedEventHandlerTestsFixture.CohortReference}/details"
                    )
                  , It.IsAny<SendOptions>()), Times.Once);
        }
    }

    public class OverlappingTrainingDateResolvedEventHandlerTestsFixture : EventHandlerTestsFixture<OverlappingTrainingDateResolvedEvent, OverlappingTrainingDateResolvedEventHandler>
    {
        public Mock<ILogger<OverlappingTrainingDateResolvedEventHandler>> Logger { get; set; }
        public Mock<IEncodingService> MockEncodingService { get; set; }
        public OverlappingTrainingDateResolvedEvent Event { get; set; }

        private readonly DraftApprenticeship _draftApprenticeship;
        private readonly ProviderCommitmentsDbContext _db;
        public const string CohortReference = "1234567899";
        public const long ProviderId = 1;
        public const string ProviderName = "TestEmployerName";
        public const string Email = "Test@education.com";
        public const string ProviderCommitmentsBaseUrl = "https://approvals/";

        public OverlappingTrainingDateResolvedEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<OverlappingTrainingDateResolvedEventHandler>>();

            var autoFixture = new Fixture();

            Event = autoFixture.Create<OverlappingTrainingDateResolvedEvent>();

            _draftApprenticeship = new DraftApprenticeship();
            _draftApprenticeship.SetValue(x => x.Id, Event.ApprenticeshipId);
            _draftApprenticeship.SetValue(x => x.CommitmentId, Event.CohortId);

            _draftApprenticeship.SetValue(x => x.Cohort, new Cohort());

            _draftApprenticeship.Cohort.SetValue(x => x.Provider, new Provider
            {

            });

            _draftApprenticeship.Cohort.SetValue(x => x.ProviderId, ProviderId);
            _draftApprenticeship.Cohort.SetValue(x => x.LastUpdatedByProviderEmail, Email);
            _draftApprenticeship.Cohort.SetValue(x => x.Reference, CohortReference);
            _draftApprenticeship.Cohort.SetValue(x => x.Id, Event.CohortId);
            _draftApprenticeship.Cohort.SetValue(x => x.WithParty, Party.Provider);
            _draftApprenticeship.Cohort.Provider.SetValue(x => x.Name, ProviderName);


            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _db.DraftApprenticeships.Add(_draftApprenticeship);
            _db.SaveChanges();

            Handler = new OverlappingTrainingDateResolvedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Logger.Object,
                new CommitmentsV2Configuration { ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl });
        }

        public override Task Handle()
        {
            return Handler.Handle(Event, MessageHandlerContext.Object);
        }
    }
}

