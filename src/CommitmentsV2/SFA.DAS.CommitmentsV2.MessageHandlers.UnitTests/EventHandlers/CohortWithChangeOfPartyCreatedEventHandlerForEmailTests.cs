using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
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
        public async Task When_HandlingEvent_IfLevyAccount_SendApproveNewEmployerDetails_Levy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.Levy).Handle();
            _fixture.VerifyEmailSent(CohortWithChangeOfPartyCreatedEventHandlerForEmail.TemplateApproveNewEmployerDetailsLevy);
        }

        [Test]
        public async Task When_HandlingEvent_IfLevyAccount_SendApproveNewEmployerDetails_NonLevy()
        {
            await _fixture.WithLevyStatus(ApprenticeshipEmployerType.NonLevy).Handle();
            _fixture.VerifyEmailSent(CohortWithChangeOfPartyCreatedEventHandlerForEmail.TemplateApproveNewEmployerDetailsNonLevy);
        }   

        public class CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture
        {
            private readonly CohortWithChangeOfPartyCreatedEventHandlerForEmail _handler;
            private readonly CohortWithChangeOfPartyCreatedEvent _event;
            private readonly Mock<IMediator> _mediator;
            private readonly TestableMessageHandlerContext _messageHandlerContext;
            private readonly Mock<IEncodingService> _encodingService;
            private readonly GetCohortSummaryQueryResult _cohortSummary;
            private readonly string _cohortReference;
            private readonly string _employerEncodedAccountId;
            private Fixture _autoFixture;

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture()
            {
                _autoFixture = new Fixture();
                _mediator = new Mock<IMediator>();

                _cohortSummary = _autoFixture.Create<GetCohortSummaryQueryResult>();
                _mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _cohortSummary);

                _cohortReference = _autoFixture.Create<string>();
                _employerEncodedAccountId = _autoFixture.Create<string>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.CohortId),
                        EncodingType.CohortReference)).Returns(_cohortReference);
                _encodingService.Setup(x => x.Encode(It.Is<long>(id => id == _cohortSummary.AccountId),
                    EncodingType.AccountId)).Returns(_employerEncodedAccountId);

                _handler = new CohortWithChangeOfPartyCreatedEventHandlerForEmail(_mediator.Object,
                    _encodingService.Object,
                    Mock.Of<ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail>>());

                _messageHandlerContext = new TestableMessageHandlerContext();
                _event = _autoFixture.Create<CohortWithChangeOfPartyCreatedEvent>();
                _event.OriginatingParty = Party.Provider;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext);
            }

            public CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithLevyStatus(ApprenticeshipEmployerType levyStatus)
            {
                _cohortSummary.LevyStatus = levyStatus;
                return this;
            }

            public void VerifyEmailSent(string templateName)
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

            internal CohortWithChangeOfPartyCreatedEventHandlerForEmailTestsFixture WithOriginatingParty(Party originatingParty)
            {
                _event.OriginatingParty = originatingParty;
                return this;
            }
         
        }
    }
}
