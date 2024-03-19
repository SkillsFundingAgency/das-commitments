using System;
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
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.Context;

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
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsEmployer_ThenProviderEmailNotSent()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.NonLevy).Handle();
            _fixture.VerifyProviderEmailNotSent();
        }

        [Test]
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsProvider_Then_EmailSentToProviderForReview()
        {
            await _fixture.WithOriginatingParty(Party.Employer).WithEmployerCompletedDetails(true).Handle();
            _fixture.VerifyProviderEmailSentForReview();
        }

        [Test]
        public async Task When_HandlingEvent_AndChangeOfPartyTypeIsProvider_Then_EmailSentToProviderWithDetailsRequired()
        {
            await _fixture.WithOriginatingParty(Party.Employer).WithEmployerCompletedDetails(false).Handle();
            _fixture.VerifyProviderEmailSentDetailsRequired();
        }

        [Test]
        public async Task When_HandlingEvent_AndIsChangeOfProvider_ThenEmployerEmailIsNotSent()
        {
            await _fixture.WithOriginatingParty(Party.Employer).Handle();
            _fixture.VerifyEmployerEmailNotSent();
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
            private readonly ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly string _expectedApprenticeName;
            private readonly string _expectedSubjectForReview;
            private readonly string _expectedSubjectDetailsRequired;
            private readonly string _expectedRequestUrl;
            private const string _expectedTemplate = "ProviderApprenticeshipChangeOfProviderRequested";
            private readonly string _cohortReference;
            private readonly string _employerEncodedAccountId;
            private readonly Fixture _autoFixture;
            public UnitOfWorkContext UnitOfWorkContext { get; set; }
            private static CommitmentsV2Configuration commitmentsV2Configuration;

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture()
            {
                _autoFixture = new Fixture();
                _mediator = new Mock<IMediator>();
                UnitOfWorkContext = new UnitOfWorkContext();

                commitmentsV2Configuration = new CommitmentsV2Configuration()
                {
                    ProviderCommitmentsBaseUrl = "https://approvals.environmentname-pas.apprenticeships.education.gov.uk/"
                };

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

                _changeOfPartyRequest = new ChangeOfPartyRequest(
                    _apprenticeship,
                    ChangeOfPartyRequestType.ChangeProvider,
                    Party.Employer,
                    _autoFixture.Create<long>(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false,
                    _autoFixture.Create<UserInfo>(),
                    DateTime.Now);
                _apprenticeship.Cohort.ChangeOfPartyRequestId = _changeOfPartyRequest.Id;

                _db.ChangeOfPartyRequests.Add(_changeOfPartyRequest);
                _db.Apprenticeships.Add(_apprenticeship);
                _db.SaveChanges();

                _expectedApprenticeName = _apprenticeship.LastName.EndsWith("s") ? $"{_apprenticeship.FirstName} {_apprenticeship.LastName}'" : $"{_apprenticeship.FirstName} {_apprenticeship.LastName}'s";
                _expectedSubjectDetailsRequired = $"{_cohortSummary.LegalEntityName} has requested that you add details on their behalf";
                _expectedSubjectForReview = $"{_cohortSummary.LegalEntityName} has added details for you to review";
                _expectedRequestUrl = $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{_cohortSummary.ProviderId}/unapproved/{_cohortSummary.CohortReference}/details";

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
                    Mock.Of<ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail>>(),
                    commitmentsV2Configuration);

                _messageHandlerContext = new TestableMessageHandlerContext();

                _event = new CohortWithChangeOfPartyCreatedEvent(_cohortSummary.CohortId, _changeOfPartyRequest.Id, Party.Employer, DateTime.Now, _autoFixture.Create<UserInfo>());
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext);
            }

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithLevyStatus(ApprenticeshipEmployerType levyStatus)
            {
                _event.OriginatingParty = Party.Provider;
                _cohortSummary.LevyStatus = levyStatus;
                return this;
            }

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithEmployerCompletedDetails(bool employerCompletedDetails)
            {
                _cohortSummary.IsCompleteForEmployer = employerCompletedDetails;
                return this;
            }

            public void VerifyEmployerEmailSent(string templateName)
            {
                var emailToEmployerCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToEmployerCommand)
                       .Select(y => y.Message as SendEmailToEmployerCommand);
                var emailToEmployerCommand = emailToEmployerCommands.First();

                Assert.That(_messageHandlerContext.SentMessages.Count(), Is.EqualTo(1));
                Assert.That(emailToEmployerCommand.AccountId, Is.EqualTo(_cohortSummary.AccountId));
                Assert.That(emailToEmployerCommand.Template, Is.EqualTo(templateName));
                Assert.That(emailToEmployerCommand.Tokens.Count(), Is.EqualTo(3));
                Assert.That(emailToEmployerCommand.Tokens["provider_name"], Is.EqualTo(_cohortSummary.ProviderName));
                Assert.That(emailToEmployerCommand.Tokens["employer_hashed_account"], Is.EqualTo(_employerEncodedAccountId));
                Assert.That(emailToEmployerCommand.Tokens["cohort_reference"], Is.EqualTo(_cohortReference));
            }

            public void VerifyEmployerEmailNotSent()
            {
                var emailToEmployerCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToEmployerCommand)
                      .Select(y => y.Message as SendEmailToEmployerCommand);

                Assert.That(emailToEmployerCommands.Count(), Is.EqualTo(0));
            }

            public void VerifyProviderEmailSentForReview()
            {
                var emailToProviderCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToProviderCommand)
                      .Select(y => y.Message as SendEmailToProviderCommand);

                var providerEmail = emailToProviderCommands.First();

                Assert.That(providerEmail.Template, Is.EqualTo(_expectedTemplate));
                Assert.That(providerEmail.Tokens["TrainingProviderName"], Is.EqualTo(_cohortSummary.ProviderName));
                Assert.That(providerEmail.Tokens["EmployerName"], Is.EqualTo(_cohortSummary.LegalEntityName));
                Assert.That(providerEmail.Tokens["RequestUrl"], Is.EqualTo(_expectedRequestUrl));
                Assert.That(providerEmail.Tokens["ApprenticeNamePossessive"], Is.EqualTo(_expectedApprenticeName));
                Assert.That(providerEmail.Tokens["Subject"], Is.EqualTo(_expectedSubjectForReview));
            }

            public void VerifyProviderEmailSentDetailsRequired()
            {
                var emailToProviderCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToProviderCommand)
                    .Select(y => y.Message as SendEmailToProviderCommand);

                var providerEmail = emailToProviderCommands.First();

                Assert.That(providerEmail.Template, Is.EqualTo(_expectedTemplate));
                Assert.That(providerEmail.Tokens["TrainingProviderName"], Is.EqualTo(_cohortSummary.ProviderName));
                Assert.That(providerEmail.Tokens["EmployerName"], Is.EqualTo(_cohortSummary.LegalEntityName));
                Assert.That(providerEmail.Tokens["RequestUrl"], Is.EqualTo(_expectedRequestUrl));
                Assert.That(providerEmail.Tokens["ApprenticeNamePossessive"], Is.EqualTo(_expectedApprenticeName));
                Assert.That(providerEmail.Tokens["Subject"], Is.EqualTo(_expectedSubjectDetailsRequired));
            }

            public void VerifyProviderEmailNotSent()
            {
                var emailToProviderCommands = _messageHandlerContext.SentMessages.Where(x => x.Message is SendEmailToProviderCommand)
                      .Select(y => y.Message as SendEmailToProviderCommand);

                Assert.That(emailToProviderCommands.Count(), Is.EqualTo(0));
            }

            internal CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithOriginatingParty(Party originatingParty)
            {
                _event.OriginatingParty = originatingParty;
                return this;
            }

        }
    }
}
