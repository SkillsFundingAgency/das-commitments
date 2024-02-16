using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortEmailOverlaps
{
    [TestFixture]
    public class GetCohortEmailOverlapsQueryHandlerTests
    {
        [Test]
        public async Task Handle_Test()
        {
            var fixtures = new GetCohortEmailOverlapsQueryHandlerTestFixtures();
            var result = await fixtures.GetResult(new GetCohortEmailOverlapsQuery(123));

            Assert.That(result.Overlaps, Has.Count.EqualTo(fixtures.OverlapResults.Count));
            Assert.Multiple(() =>
            {
                Assert.That(result.Overlaps[0].Id, Is.EqualTo(fixtures.OverlapResults[0].RowId));
                Assert.That(result.Overlaps[0].ErrorMessage, Is.EqualTo(fixtures.OverlapResults[0].BuildErrorMessage()));
                Assert.That(result.Overlaps[1].Id, Is.EqualTo(fixtures.OverlapResults[1].RowId));
                Assert.That(result.Overlaps[1].ErrorMessage, Is.EqualTo(fixtures.OverlapResults[1].BuildErrorMessage()));
            });
        }
    }

    public class GetCohortEmailOverlapsQueryHandlerTestFixtures
    {
        private readonly Fixture _autoFixture;
        public GetCohortEmailOverlapsQueryHandlerTestFixtures()
        {
            _autoFixture = new Fixture();
            OverlapResults = _autoFixture.CreateMany<EmailOverlapCheckResult>().ToList();

            OverlapCheckServiceMock = new Mock<IOverlapCheckService>();
            OverlapCheckServiceMock.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OverlapResults);
        }

        public List<EmailOverlapCheckResult> OverlapResults { get; set; }

        private Mock<IOverlapCheckService> OverlapCheckServiceMock { get; set; }

        public Task<GetCohortEmailOverlapsQueryResult> GetResult(GetCohortEmailOverlapsQuery query)
        {
            var handler = new GetCohortEmailOverlapsQueryHandler(OverlapCheckServiceMock.Object);

            return handler.Handle(query, CancellationToken.None);
        }
    }
}