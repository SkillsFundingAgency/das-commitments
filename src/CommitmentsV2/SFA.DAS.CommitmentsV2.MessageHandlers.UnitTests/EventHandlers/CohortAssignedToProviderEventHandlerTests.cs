using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class CohortAssignedToProviderEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenCalled_ThenShouldCallGetCohortSummaryQueryWithCorrectCohortId()
        {
            var fixture = new CohortAssignedToProviderEventHandlerTestsFixture().SetupNonTransferCohort();
            await fixture.Handle();

            fixture.Mediator.Verify(x=>x.Send(It.Is<GetCohortSummaryQuery>(c=>c.CohortId == fixture.Message.CohortId), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Handle_WhenCalledAndCohortIsNonTransfer_ThenShouldBuildEmailRequestAndSendIt()
        {
            var fixture = new CohortAssignedToProviderEventHandlerTestsFixture().SetupNonTransferCohort();
            await fixture.Handle();

            fixture.VerfiyProviderEmailRequestIsCreatedAndSentCorrectly(fixture.GetCohortSummaryQueryResult.LastAction);
        }

        [Test]
        public async Task Handle_WhenCalledAndCohortIsTransfer_ThenShouldBuildEmailRequestAndSendIt()
        {
            var fixture = new CohortAssignedToProviderEventHandlerTestsFixture().SetupTransferCohort();
            await fixture.Handle();

            fixture.VerfiyProviderTransferEmailRequestIsCreatedAndSentCorrectly(fixture.GetCohortSummaryQueryResult.LastAction);
        }

        [Test]
        public async Task Handle_WhenCalledAndCohortIsChangeOfProvider_ThenEmailShouldNotBeSent()
        {
            var fixture = new CohortAssignedToProviderEventHandlerTestsFixture().SetupChangeOfProviderCohort();
            await fixture.Handle();

            fixture.VerifyProviderAssignedEmailIsNotSent();
        }

        [Test]
        public void Handle_WhenCohortSummaryIsNull_ThenShould_NotSendEmail()
        {
            var fixture = new CohortAssignedToProviderEventHandlerTestsFixture().SetupNonTransferCohort();
            fixture.Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCohortSummaryQueryResult)null);

            Assert.DoesNotThrowAsync(fixture.Handle);

            fixture.VerifyProviderAssignedEmailIsNotSent();
        }
    }

    public class CohortAssignedToProviderEventHandlerTestsFixture : EventHandlerTestsFixture<CohortAssignedToProviderEvent, CohortAssignedToProviderEventHandler>
    {
        public Mock<IApprovalsOuterApiClient> ApprovalsOuterApiClient { get; }
        public Mock<ILogger<CohortAssignedToProviderEventHandler>> Logger { get; }

        public GetCohortSummaryQueryResult GetCohortSummaryQueryResult { get; private set; }

        public CohortAssignedToProviderEventHandlerTestsFixture() : base((m) => null)
        {
            ApprovalsOuterApiClient = new Mock<IApprovalsOuterApiClient>();
            Logger = new Mock<ILogger<CohortAssignedToProviderEventHandler>>();

            Handler = new CohortAssignedToProviderEventHandler(Mediator.Object, ApprovalsOuterApiClient.Object, Logger.Object);
        }

        public CohortAssignedToProviderEventHandlerTestsFixture SetupNonTransferCohort()
        {
            GetCohortSummaryQueryResult = DataFixture.Build<GetCohortSummaryQueryResult>()
                .With(p => p.CohortId, Message.CohortId).Without(p => p.TransferSenderId).Without(p => p.ChangeOfPartyRequestId).Create();

            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetCohortSummaryQueryResult);

            return this;
        }

        public CohortAssignedToProviderEventHandlerTestsFixture SetupTransferCohort()
        {
            GetCohortSummaryQueryResult = DataFixture.Build<GetCohortSummaryQueryResult>()
                .With(p => p.CohortId, Message.CohortId).With(p => p.TransferSenderId, 12345).Without(p => p.ChangeOfPartyRequestId).Create();

            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetCohortSummaryQueryResult);

            return this;
        }

        public CohortAssignedToProviderEventHandlerTestsFixture SetupChangeOfProviderCohort()
        {
            GetCohortSummaryQueryResult = DataFixture.Build<GetCohortSummaryQueryResult>()
                .With(p => p.ChangeOfPartyRequestId, 1000).Create();

            Mediator.Setup(x => x.Send(It.IsAny<GetCohortSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetCohortSummaryQueryResult);

            return this;
        }
        public void VerfiyProviderEmailRequestIsCreatedAndSentCorrectly(LastAction lastAction)
        {
            var actionType = lastAction == LastAction.Approve ? "approval" : "review";

            ApprovalsOuterApiClient.Verify(x => x.PostWithResponseCode<ProviderEmailRequest, object>(It.Is<PostProviderEmailRequest>(p =>
                    p.Data.TemplateId == "ProviderCommitmentNotification" &&
                    p.Data.ExplicitEmailAddresses[0] == GetCohortSummaryQueryResult.LastUpdatedByProviderEmail &&
                    p.Data.Tokens["cohort_reference"] == GetCohortSummaryQueryResult.CohortReference &&
                    p.Data.Tokens["type"] == actionType
                ), false));
        }

        public void VerfiyProviderTransferEmailRequestIsCreatedAndSentCorrectly(LastAction lastAction)
        {
            var actionType = lastAction == LastAction.Approve ? "approval" : "review";

            ApprovalsOuterApiClient.Verify(x => x.PostWithResponseCode<ProviderEmailRequest, object>(
                It.Is<PostProviderEmailRequest>(p =>
                    p.Data.TemplateId == "ProviderTransferCommitmentNotification" &&
                    p.Data.ExplicitEmailAddresses[0] == GetCohortSummaryQueryResult.LastUpdatedByProviderEmail &&
                    p.Data.Tokens["cohort_reference"] == GetCohortSummaryQueryResult.CohortReference &&
                    p.Data.Tokens["employer_name"] == GetCohortSummaryQueryResult.LegalEntityName &&
                    p.Data.Tokens["type"] == actionType
                ), false));
        }

        public void VerifyProviderAssignedEmailIsNotSent()
        {
            ApprovalsOuterApiClient.Verify(x => x.PostWithResponseCode<ProviderEmailRequest, object>(It.IsAny<IPostApiRequest<ProviderEmailRequest>>(), false),
                Times.Never);
        }
    }
}