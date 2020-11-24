using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class CohortWithChangeOfPartyCreatedEventHandlerForEmailTests
    {
        public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture();
        }

        [Test]
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsEmployer_IfLevyAccount_SendApproveNewEmployerDetails_Levy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.Levy).Handle();
            _fixture.VerifyEmployerEmailSent(CohortWithChangeOfPartyCreatedEventHandlerForEmail.TemplateApproveNewEmployerDetailsLevy);
        }

        [Test]
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsEmployer_IfLevyAccount_SendApproveNewEmployerDetails_NonLevy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.NonLevy).Handle();
            _fixture.VerifyEmployerEmailSent(CohortWithChangeOfPartyCreatedEventHandlerForEmail.TemplateApproveNewEmployerDetailsNonLevy);
        }   

        [Test]
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsProvider_Then_EmailSentToProvider()
        {
            await _fixture.ChangeProvider().Handle();
            _fixture.VerifyProviderEmailSent();
        }

        public class CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture
        {
            private readonly CohortWithChangeOfPartyCreatedEventHandlerForEmail _handler;
            private readonly CohortWithChangeOfPartyCreatedEvent _event;

            private readonly ProviderCommitmentsDbContext _db;
            private readonly Mock<IMediator> _mediator;
            private readonly TestableMessageHandlerContext _messageHandlerContext;
            private readonly Mock<IEncodingService> _encodingService;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private readonly Apprenticeship _apprenticeship;
            private readonly string _expectedApprenticeName;
            private readonly string _expectedSubject;
            private readonly string _expectedRequestUrl;
            private const string _expectedTemplate = "ProviderApprenticeshipChangeOfProviderRequested";
            private readonly string _cohortReference;
            private readonly string _employerEncodedAccountId;
            private Fixture _autoFixture;

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture()
            {
                _autoFixture = new Fixture();
                _mediator = new Mock<IMediator>();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

                _cohortSummary = _autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _cohortSummary);

                _apprenticeship = new Apprenticeship
                {
                    Id = _autoFixture.Create<long>(),
                    FirstName = _autoFixture.Create<string>(),
                    LastName = _autoFixture.Create<string>(),
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = new AccountLegalEntity()
                    },
                    ReservationId = _autoFixture.Create<Guid>()
                };

                _db.Apprenticeships.Add(_apprenticeship);
                _db.SaveChanges();

                _expectedApprenticeName = _apprenticeship.LastName.EndsWith("s") ? $"{_apprenticeship.FirstName} {_apprenticeship.LastName}'" : $"{_apprenticeship.FirstName} {_apprenticeship.LastName}'s";
                _expectedSubject = $"{_cohortSummary.LegalEntityName} has requested that you add details on their behalf";
                _expectedRequestUrl = $"{_cohortSummary.ProviderId}/apprentices/{_cohortSummary.CohortReference}/details";

                _cohortReference = _autoFixture.Create<string>();
                _employerEncodedAccountId = _autoFixture.Create<string>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.CohortId),
                        EncodingType.CohortReference)).Returns(_cohortReference);
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.AccountId),
                    EncodingType.AccountId)).Returns(_employerEncodedAccountId);

                _handler = new CohortWithChangeOfPartyCreatedEventHandlerForEmail(new Lazy<ProviderCommitmentsDbContext>(() => _db),
                    _mediator.Object,
                    _encodingService.Object,
                    Mock.Of<ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail>>());

                _messageHandlerContext = new TestableMessageHandlerContext();
                _event = _autoFixture.Create<CohortWithChangeOfPartyCreatedEvent>();
                _event.OriginatingParty = Party.Provider;
                _event.ApprenticeshipId = _apprenticeship.Id;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext);
            }

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithLevyStatus(ApprenticeshipEmployerType levyStatus)
            {
                _event.ChangeOfPartyType = ChangeOfPartyRequestType.ChangeEmployer;
                _cohortSummary.LevyStatus = levyStatus;
                return this;
            }

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture ChangeProvider()
            {
                _event.ChangeOfPartyType = ChangeOfPartyRequestType.ChangeProvider;
                return this;
            }

            public void VerifyEmployerEmailSent(string templateName)
            {
               var emailToEmployerCommands =  _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToEmployerCommand)
                      .Select(y => y.Message as SendEmailToEmployerCommand);
                var emailToEmployerCommand = emailToEmployerCommands.First();

                Assert.AreEqual(1, _messageHandlerContext.SentMessages.Count());
                Assert.AreEqual(_cohortSummary.AccountId, emailToEmployerCommand.AccountId);
                Assert.AreEqual(templateName, emailToEmployerCommand.Template);
                Assert.AreEqual(3, emailToEmployerCommand.Tokens.Count());
                Assert.AreEqual(_cohortSummary.ProviderName, emailToEmployerCommand.Tokens["provider_name"]);
                Assert.AreEqual(_employerEncodedAccountId, emailToEmployerCommand.Tokens["employer_hashed_account"]);
                Assert.AreEqual(_cohortReference, emailToEmployerCommand.Tokens["cohort_reference"]);
            }

            public void VerifyProviderEmailSent()
            {
                var emailToProviderCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToProviderCommand)
                      .Select(y => y.Message as SendEmailToProviderCommand);

                var providerEmail = emailToProviderCommands.First();

                Assert.AreEqual(_expectedTemplate, providerEmail.Template);
                Assert.AreEqual(_cohortSummary.ProviderName, providerEmail.Tokens["TrainingProviderName"]);
                Assert.AreEqual(_cohortSummary.LegalEntityName, providerEmail.Tokens["EmployerName"]);
                Assert.AreEqual(_expectedRequestUrl, providerEmail.Tokens["RequestUrl"]);
                Assert.AreEqual(_expectedApprenticeName, providerEmail.Tokens["ApprenticeNamePossessive"]);
                Assert.AreEqual(_expectedSubject, providerEmail.Tokens["Subject"]);
            }

            internal CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithOriginatingParty(Party originatingParty)
            {
                _event.OriginatingParty = originatingParty;
                return this;
            }
         
        }
    }
}
