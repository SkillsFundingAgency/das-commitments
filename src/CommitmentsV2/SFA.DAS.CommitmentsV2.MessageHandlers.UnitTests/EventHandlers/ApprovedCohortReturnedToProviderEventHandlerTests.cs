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
    public class ApprovedCohortReturnedToProviderEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenApprovedCohortReturnedToProviderEventIsRaised_ThenShouldGetCorrectCohort()
        {
            var fixture = new ApprovedCohortReturnedToProviderEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.VerifyGetCohortSummaryIsCorrectlyCalled();
        }

        [Test]
        public async Task Handle_WhenNoCohortReturned_ThenShouldSwallowEvent()
        {
            var fixture = new ApprovedCohortReturnedToProviderEventHandlerTestsFixture().WithNoCohort();
            await fixture.Handle();
            fixture.VerifyGetCohortSummaryIsCorrectlyCalled();
        }

        [Test]
        public async Task Handle_WhenApprovedCohortReturnedToProviderEventIsRaised_ThenShouldRelayMessageToAzureServiceBus()
        {
            var fixture = new ApprovedCohortReturnedToProviderEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprovedCohortReturnedToProviderEventHandlerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public ApprovedCohortReturnedToProviderEventHandler Sut;
        public ApprovedCohortReturnedToProviderEvent ApprovedCohortReturnedToProviderEvent;
        public GetCohortSummaryQueryResult GetCohortSummaryQueryResult;

        public ApprovedCohortReturnedToProviderEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new ApprovedCohortReturnedToProviderEventHandler(Mediator.Object, LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<ApprovedCohortReturnedToProviderEventHandler>>());
            ApprovedCohortReturnedToProviderEvent = autoFixture.Create<ApprovedCohortReturnedToProviderEvent>();

            GetCohortSummaryQueryResult = autoFixture.Build<GetCohortSummaryQueryResult>().Create();

            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetCohortSummaryQueryResult);
        }

        public ApprovedCohortReturnedToProviderEventHandlerTestsFixture WithNoCohort()
        {
            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCohortSummaryQueryResult)null);
            return this;
        }

        public Task Handle()
        {
            return Sut.Handle(ApprovedCohortReturnedToProviderEvent, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyGetCohortSummaryIsCorrectlyCalled()
        {
            Mediator.Verify(x =>
                x.Send(It.Is<GetCohortSummaryQuery>(p => p.CohortId == ApprovedCohortReturnedToProviderEvent.CohortId),
                    It.IsAny<CancellationToken>()));
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ApprovedCohortReturnedToProvider>(p =>
                p.AccountId == GetCohortSummaryQueryResult.AccountId &&
                p.ProviderId == GetCohortSummaryQueryResult.ProviderId &&
                p.CommitmentId == ApprovedCohortReturnedToProviderEvent.CohortId)));
        }
    }
}
