using Microsoft.Extensions.Logging;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CohortApprovedByEmployerEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenApprovedCohortReturnedToProviderEventIsRaised_ThenShouldGetCorrectCohort()
        {
            var fixture = new CohortApprovedByEmployerEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.VerifyGetCohortSummaryIsCorrectlyCalled();
        }
        [Test]

        public async Task Handle_WhenApprovedCohortReturnedToProviderEventIsRaised_ThenShouldRelayMessageToAzureServiceBus()
        {
            var fixture = new CohortApprovedByEmployerEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class CohortApprovedByEmployerEventHandlerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public CohortApprovedByEmployerEventHandler Sut;
        public CohortApprovedByEmployerEvent CohortApprovedByEmployerEvent;
        public GetCohortSummaryQueryResult GetCohortSummaryQueryResult;

        public CohortApprovedByEmployerEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new CohortApprovedByEmployerEventHandler(Mediator.Object, LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<CohortApprovedByEmployerEventHandler>>());
            CohortApprovedByEmployerEvent = autoFixture.Create<CohortApprovedByEmployerEvent>();

            GetCohortSummaryQueryResult = autoFixture.Build<GetCohortSummaryQueryResult>().Create();

            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetCohortSummaryQueryResult);
        }

        public Task Handle()
        {
            return Sut.Handle(CohortApprovedByEmployerEvent, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyGetCohortSummaryIsCorrectlyCalled()
        {
            Mediator.Verify(x =>
                x.Send(It.Is<GetCohortSummaryQuery>(p => p.CohortId == CohortApprovedByEmployerEvent.CohortId),
                    It.IsAny<CancellationToken>()));
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovedByEmployer>(p =>
                p.AccountId == GetCohortSummaryQueryResult.AccountId &&
                p.ProviderId == GetCohortSummaryQueryResult.ProviderId &&
                p.CommitmentId == CohortApprovedByEmployerEvent.CohortId)));
        }
    }
}
