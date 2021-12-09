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
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetTests : FluentTest<GetTestsFixture>
    {
        [Test]
        public async Task WhenGetRequestReceived_ThenShouldReturnOkayResponse()
        {
            await TestAsync(
                f => f.Get(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetCohortResponse>(v =>
                        v.CohortId == f.GetCohortResult.CohortId &&
                        v.AccountLegalEntityId == f.GetCohortResult.AccountLegalEntityId &&
                        v.LegalEntityName == f.GetCohortResult.LegalEntityName &&
                        v.ProviderName == f.GetCohortResult.ProviderName &&
                        v.IsFundedByTransfer == f.GetCohortResult.IsFundedByTransfer &&
                        v.TransferSenderId == f.GetCohortResult.TransferSenderId &&
                        v.PledgeApplicationId == f.GetCohortResult.PledgeApplicationId &&
                        v.WithParty == f.GetCohortResult.WithParty &&
                        v.LatestMessageCreatedByEmployer == f.GetCohortResult.LatestMessageCreatedByEmployer &&
                        v.LatestMessageCreatedByProvider == f.GetCohortResult.LatestMessageCreatedByProvider &&
                        v.IsApprovedByEmployer == f.GetCohortResult.IsApprovedByEmployer &&
                        v.IsApprovedByProvider == f.GetCohortResult.IsApprovedByProvider &&
                        v.LevyStatus == f.GetCohortResult.LevyStatus &&
                        v.LastAction == f.GetCohortResult.LastAction &&
                        v.ApprenticeEmailIsRequired == f.GetCohortResult.ApprenticeEmailIsRequired &&
                        v.TransferApprovalStatus == f.GetCohortResult.TransferApprovalStatus));
        }

        [Test]
        public async Task WhenGetRequestReceivedForNonExistentCohort_ThenShouldReturnNotFoundResponse()
        {
            await TestAsync(
                f => f.Get(987298),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<NotFoundResult>());
        }

        [Test]
        public async Task WhenGetCohortsRequestReceivedForEmployer_ThenShouldReturnOkayResponseWithCohorts()
        {
            await TestAsync(
                f => f.GetCohorts(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetCohortsResponse>(v =>
                        v.Cohorts.Length == f.GetCohortsResult.Cohorts.Length));
        }

        [Test]
        public async Task WhenGetCohortsRequestReceivedForEmployerAndNoCohortsFound_ThenShouldReturnOkResponseWithNoCohorts()
        {
            await TestAsync(
                f => f.WithNoCohortsForEmployer().GetCohorts(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetCohortsResponse>(v =>
                        v.Cohorts.Length == 0));
        }

        [Test]
        public async Task WhenGetCohortEmailOverlaps_ThenShouldReturnOkResponseWithList()
        {
            await TestAsync(
                f => f.GetEmailOverlaps(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetEmailOverlapsResponse>(v =>
                        v.ApprenticeshipEmailOverlaps.ToList().Count == f.GetCohortEmailOverlapsResult.Overlaps.Count));
        }
    }

    public class GetTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public GetCohortSummaryQueryResult GetCohortResult { get; }
        public GetCohortEmailOverlapsQueryResult GetCohortEmailOverlapsResult { get; }
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
            Mediator.Setup(m => m.Send(It.Is<GetCohortSummaryQuery>(q => q.CohortId == CohortId), CancellationToken.None)).ReturnsAsync(GetCohortResult);
            Mediator.Setup(m => m.Send(It.Is<GetCohortEmailOverlapsQuery>(q => q.CohortId == CohortId), CancellationToken.None)).ReturnsAsync(GetCohortEmailOverlapsResult);

            GetCohortsRequest = AutoFixture.Build<GetCohortsRequest>().With(x => x.AccountId, AccountId).Create();
            GetCohortsResult = AutoFixture.Create<GetCohortsResult>();
            Mediator.Setup(m => m.Send(It.Is<GetCohortsQuery>(q => q.AccountId == AccountId), CancellationToken.None)).ReturnsAsync(GetCohortsResult);
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

        public GetTestsFixture WithNoCohortsForEmployer()
        {
            Mediator.Setup(m => m.Send(It.Is<GetCohortsQuery>(q => q.AccountId == AccountId), CancellationToken.None))
                .ReturnsAsync(new GetCohortsResult(new List<CohortSummary>()));
            return this;
        }
    }
}