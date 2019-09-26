using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetTests : FluentTest<GetTestsFixture>
    {
        [Test]
        public async Task WhenGetRequestReceived_ThenShouldReturnResponse()
        {
            await TestAsync(
                f => f.Get(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetCohortResponse>(v =>
                        v.CohortId == f.Result.CohortId &&
                        v.AccountLegalEntityId == f.Result.AccountLegalEntityId &&
                        v.LegalEntityName == f.Result.LegalEntityName &&
                        v.ProviderName == f.Result.ProviderName &&
                        v.IsFundedByTransfer == f.Result.IsFundedByTransfer &&
                        v.TransferSenderId == f.Result.TransferSenderId &&
                        v.WithParty == f.Result.WithParty &&
                        v.LatestMessageCreatedByEmployer == f.Result.LatestMessageCreatedByEmployer &&
                        v.LatestMessageCreatedByProvider == f.Result.LatestMessageCreatedByProvider));
        }
    }

    public class GetTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public GetCohortSummaryQueryResult Result { get; }

        private const long CohortId = 123;

        public GetTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);
            Result = AutoFixture.Create<GetCohortSummaryQueryResult>();
            
            Mediator.Setup(m => m.Send(It.Is<GetCohortSummaryQuery>(q => q.CohortId == CohortId), CancellationToken.None)).ReturnsAsync(Result);
        }

        public Task<IActionResult> Get()
        {
            return Controller.Get(CohortId);
        }
    }
}