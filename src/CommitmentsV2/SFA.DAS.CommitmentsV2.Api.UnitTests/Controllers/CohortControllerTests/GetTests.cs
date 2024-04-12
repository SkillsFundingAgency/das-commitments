using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetTests
    {
        [Test]
        public async Task WhenGetRequestReceived_ThenShouldReturnOkayResponse()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.Get();

            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<GetCohortResponse>(response =>
                    response.CohortId == fixture.GetCohortResult.CohortId &&
                    response.AccountLegalEntityId == fixture.GetCohortResult.AccountLegalEntityId &&
                    response.LegalEntityName == fixture.GetCohortResult.LegalEntityName &&
                    response.ProviderId == fixture.GetCohortResult.ProviderId &&
                    response.ProviderName == fixture.GetCohortResult.ProviderName &&
                    response.IsFundedByTransfer == fixture.GetCohortResult.IsFundedByTransfer &&
                    response.TransferSenderId == fixture.GetCohortResult.TransferSenderId &&
                    response.PledgeApplicationId == fixture.GetCohortResult.PledgeApplicationId &&
                    response.WithParty == fixture.GetCohortResult.WithParty &&
                    response.LatestMessageCreatedByEmployer == fixture.GetCohortResult.LatestMessageCreatedByEmployer &&
                    response.LatestMessageCreatedByProvider == fixture.GetCohortResult.LatestMessageCreatedByProvider &&
                    response.IsApprovedByEmployer == fixture.GetCohortResult.IsApprovedByEmployer &&
                    response.IsApprovedByProvider == fixture.GetCohortResult.IsApprovedByProvider &&
                    response.LevyStatus == fixture.GetCohortResult.LevyStatus &&
                    response.LastAction == fixture.GetCohortResult.LastAction &&
                    response.TransferApprovalStatus == fixture.GetCohortResult.TransferApprovalStatus);
        }

        [Test]
        public async Task WhenGetRequestReceivedForNonExistentCohort_ThenShouldReturnNotFoundResponse()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.Get(987298);

            result.Should().NotBeNull()
                .And.BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task WhenGetCohortsRequestReceivedForEmployer_ThenShouldReturnOkayResponseWithCohorts()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.GetCohorts();

            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<GetCohortsResponse>(v =>
                    v.Cohorts.Length == fixture.GetCohortsResult.Cohorts.Length);
        }

        [Test]
        public async Task WhenGetCohortsRequestReceivedForEmployerAndNoCohortsFound_ThenShouldReturnOkResponseWithNoCohorts()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.WithNoCohortsForEmployer().GetCohorts();

            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<GetCohortsResponse>(v =>
                    v.Cohorts.Length == 0);
        }

        [Test]
        public async Task WhenGetCohortEmailOverlaps_ThenShouldReturnOkResponseWithList()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.GetEmailOverlaps();

            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<GetEmailOverlapsResponse>(v =>
                    v.ApprenticeshipEmailOverlaps.ToList().Count == fixture.GetCohortEmailOverlapsResult.Overlaps.Count);
        }


        [Test]
        public async Task WhenGetGetCohortPriorLearningErrors_ThenShouldReturnOkResponseWithList()
        {
            var fixture = new GetTestsFixture();
            var result = await fixture.GetCohortPriorLearningErrors();
            
            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<GetCohortPriorLearningErrorResponse>(v =>
                    v.DraftApprenticeshipIds.ToList().Count ==
                    fixture.GetCohortPriorLearningErrorResult.DraftApprenticeshipIds.Count());
        }
    }

    public class GetTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public GetCohortSummaryQueryResult GetCohortResult { get; }
        public GetCohortEmailOverlapsQueryResult GetCohortEmailOverlapsResult { get; }
        public GetCohortPriorLearningErrorQueryResult GetCohortPriorLearningErrorResult { get; }
        public GetCohortsRequest GetCohortsRequest { get; }
        public GetCohortsResult GetCohortsResult { get; }

        public long AccountId = 1;
        private const long CohortId = 123;

        public GetTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);

            GetCohortResult = AutoFixture.Create<GetCohortSummaryQueryResult>();
            GetCohortEmailOverlapsResult = AutoFixture.Create<GetCohortEmailOverlapsQueryResult>();
            GetCohortPriorLearningErrorResult = AutoFixture.Create<GetCohortPriorLearningErrorQueryResult>();
            Mediator.Setup(m =>
                    m.Send(It.Is<GetCohortSummaryQuery>(q => q.CohortId == CohortId), CancellationToken.None))
                .ReturnsAsync(GetCohortResult);
            Mediator.Setup(m =>
                    m.Send(It.Is<GetCohortEmailOverlapsQuery>(q => q.CohortId == CohortId), CancellationToken.None))
                .ReturnsAsync(GetCohortEmailOverlapsResult);
            Mediator.Setup(m => m.Send(It.Is<GetCohortPriorLearningErrorQuery>(q => q.CohortId == CohortId),
                CancellationToken.None)).ReturnsAsync(GetCohortPriorLearningErrorResult);


            GetCohortsRequest = AutoFixture.Build<GetCohortsRequest>().With(x => x.AccountId, AccountId).Create();
            GetCohortsResult = AutoFixture.Create<GetCohortsResult>();
            Mediator.Setup(m => m.Send(It.Is<GetCohortsQuery>(q => q.AccountId == AccountId), CancellationToken.None))
                .ReturnsAsync(GetCohortsResult);
        }

        public Task<IActionResult> Get(long? id = null)
        {
            var cohortId = id ?? CohortId;

            return Controller.Get(cohortId);
        }

        public Task<IActionResult> GetCohorts()
        {
            return Controller.GetCohorts(GetCohortsRequest);
        }

        public Task<IActionResult> GetEmailOverlaps()
        {
            return Controller.GetEmailOverlapChecks(CohortId);
        }

        public Task<IActionResult> GetCohortPriorLearningErrors()
        {
            return Controller.GetCohortPriorLearningErrors(CohortId);
        }

        public GetTestsFixture WithNoCohortsForEmployer()
        {
            Mediator.Setup(m => m.Send(It.Is<GetCohortsQuery>(q => q.AccountId == AccountId), CancellationToken.None))
                .ReturnsAsync(new GetCohortsResult(new List<CohortSummary>()));
            return this;
        }
    }
}