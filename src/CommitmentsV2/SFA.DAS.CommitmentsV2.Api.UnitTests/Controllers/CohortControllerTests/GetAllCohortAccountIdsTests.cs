using System.Collections.Generic;
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
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllCohortAccountIds;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetAllCohortAccountIdsTests : FluentTest<GetAllCohortAccountIdsTestsFixture>
    {
     
        [Test]
        public async Task WhenGetAllCohortAccountIds_ThenShouldReturnOkayResponseWithAllCohortAccountIds()
        {
            await TestAsync(
                f => f.GetAllCohortAccountIds(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<GetAllCohortAccountIdsResponse>(v =>
                        v.AccountIds.Count == f.AccountIdsQueryResult.AccountIds.Count));
        }

    }

    public class GetAllCohortAccountIdsTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public GetAllCohortAccountIdsQueryResult AccountIdsQueryResult { get; set; }

        public GetAllCohortAccountIdsTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);

            AccountIdsQueryResult = AutoFixture.Create<GetAllCohortAccountIdsQueryResult>();

            Mediator
                .Setup(m => m.Send(It.IsAny<GetAllCohortAccountIdsQuery>(), CancellationToken.None))
                .ReturnsAsync(AccountIdsQueryResult);
        }

        public Task<IActionResult> GetAllCohortAccountIds()
        {
            return Controller.GetAllCohortAccountIds();
        }

    }
}