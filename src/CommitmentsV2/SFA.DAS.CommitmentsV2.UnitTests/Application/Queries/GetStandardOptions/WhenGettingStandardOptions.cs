using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetStandardOptions
{
    public class WhenGettingStandardOptions
    {
        private GetStandardOptionsHandlerTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetStandardOptionsHandlerTestFixture();

            _fixture.SeedDb();
        }

        [Test]
        public async Task Then_ReturnStandardOptionsInAlphabeticalOrder()
        {
            var result = await _fixture.Handle("ST0001_1.0");

            result.Options.Should().BeInAscendingOrder();
        }
    }

    class GetStandardOptionsHandlerTestFixture
    {
        private ProviderCommitmentsDbContext _dbContext;

        private GetStandardOptionsHandler _handler;

        public GetStandardOptionsHandlerTestFixture()
        {
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            _handler = new GetStandardOptionsHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext));
        }

        public Task<GetStandardOptionsResult> Handle(string standardUId)
        {
            return _handler.Handle(new GetStandardOptionsQuery(standardUId), CancellationToken.None);
        }

        public GetStandardOptionsHandlerTestFixture SeedDb()
        {
            var standardOptions = new List<StandardOption>
            {
                new StandardOption { StandardUId = "ST0001_1.0", Option = "Option A" },
                new StandardOption { StandardUId = "ST0001_1.0", Option = "Option C" },
                new StandardOption { StandardUId = "ST0001_1.0", Option = "Option B" },
                new StandardOption { StandardUId = "ST0002_1.0", Option = "Standard 2 Option A" }
            };

            _dbContext.StandardOptions.AddRange(standardOptions);
            _dbContext.SaveChanges();

            return this;
        }
    }
}
