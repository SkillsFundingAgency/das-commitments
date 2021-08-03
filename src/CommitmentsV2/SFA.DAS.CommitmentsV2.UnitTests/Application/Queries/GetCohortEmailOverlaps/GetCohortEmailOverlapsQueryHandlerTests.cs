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
            var f = new GetCohortEmailOverlapsQueryHandlerTestFixtures();
            var result = await f.GetResult(new GetCohortEmailOverlapsQuery(123));

            Assert.AreEqual(f.OverlapResults.Count, result.Overlaps.Count);
            Assert.AreEqual(f.OverlapResults[0].RowId, result.Overlaps[0].Id);
            Assert.AreEqual(f.OverlapResults[0].BuildErrorMessage(), result.Overlaps[0].ErrorMessage);
            Assert.AreEqual(f.OverlapResults[1].RowId, result.Overlaps[1].Id);
            Assert.AreEqual(f.OverlapResults[1].BuildErrorMessage(), result.Overlaps[1].ErrorMessage);
        }
    }

    public class GetCohortEmailOverlapsQueryHandlerTestFixtures
    {
        private Fixture _autoFixture;
        public GetCohortEmailOverlapsQueryHandlerTestFixtures()
        {
            _autoFixture = new Fixture();
            OverlapResults = _autoFixture.CreateMany<EmailOverlapCheckResult>().ToList();

            OverlapCheckServiceMock = new Mock<IOverlapCheckService>();
            OverlapCheckServiceMock.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OverlapResults);
        }

        public List<EmailOverlapCheckResult> OverlapResults { get; set; }
        public Mock<IRequestHandler<GetCohortEmailOverlapsQuery, GetCohortEmailOverlapsQueryResult>> HandlerMock { get; set; }
        public IRequestHandler<GetCohortEmailOverlapsQuery, GetCohortEmailOverlapsQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetCohortEmailOverlapsQuery>> ValidatorMock { get; set; }
        public Mock<IOverlapCheckService> OverlapCheckServiceMock { get; set; }
        public IValidator<GetCohortEmailOverlapsQuery> Validator => ValidatorMock.Object;

        public Task<GetCohortEmailOverlapsQueryResult> GetResult(GetCohortEmailOverlapsQuery query)
        {
            var handler = new GetCohortEmailOverlapsQueryHandler(OverlapCheckServiceMock.Object);

            return handler.Handle(query, CancellationToken.None);
        }
    }
}