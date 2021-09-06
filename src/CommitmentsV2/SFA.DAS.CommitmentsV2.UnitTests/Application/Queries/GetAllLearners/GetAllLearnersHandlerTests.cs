using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllLearners
{
    public class GetAllLearnersHandlerTests
    {
        /*
        private GetAllLearnersHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetAllLearnersHandlerTestsFixture();
        }

        [Test]
        public async Task WhenGettingAllLearners_ThenLearnersAreMappedToLearnerResponse()
        {
            _fixture.AddApprenticeships().SeedDb();

            var result = await _fixture.Handle();

            Assert.AreEqual(3, result.Learners.Count);
        }
    }

    class GetAllLearnersHandlerTestsFixture
    {
        private readonly GetAllLearnersQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _dbContext;

        public List<Apprenticeship> Apprenticeships { get; set; }

        public GetAllLearnersHandlerTestsFixture()
        {
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _handler = new GetAllLearnersQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext));
        }

        public GetAllLearnersHandlerTestsFixture AddApprenticeships()
        {
            Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { FirstName = "Matt", LastName = "Daniels" },
            };

            return this;
        }

        public GetAllLearnersHandlerTestsFixture SeedDb()
        {
            _dbContext.Apprenticeships.AddRange(Apprenticeships);
            _dbContext.SaveChanges();

            return this;
        }

        public Task<GetAllLearnersQueryResult> Handle()
        {
            return _handler.Handle(new GetAllLearnersQuery(), CancellationToken.None);
        }
        */
    }
}
