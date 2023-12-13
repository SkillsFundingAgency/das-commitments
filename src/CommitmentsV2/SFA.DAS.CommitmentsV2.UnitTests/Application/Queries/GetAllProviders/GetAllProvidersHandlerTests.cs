using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllProviders
{
    public class GetAllProvidersHandlerTests
    {
        private GetAllProvidersHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetAllProvidersHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task WhenGettingAllProviders_ThenProvidersAreMappedToProviderResponse()
        {
            _fixture.AddProviders().SeedDb();

            var result = await _fixture.Handle();

            Assert.That(result.Providers.Count, Is.EqualTo(3));
        }
    }

    class GetAllProvidersHandlerTestsFixture : IDisposable
    {
        private readonly GetAllProvidersQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _dbContext;

        public List<Provider> Providers { get; set; }

        public GetAllProvidersHandlerTestsFixture()
        {
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            _handler = new GetAllProvidersQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext));
        }

        public GetAllProvidersHandlerTestsFixture AddProviders()
        {
            Providers = new List<Provider>
            {
                new Provider { UkPrn = 10000001, Name = "Provider 1"},
                new Provider { UkPrn = 10000002, Name = "Provider 2"},
                new Provider { UkPrn = 10000003, Name = "Provider 3"}
            };

            return this;
        }

        public GetAllProvidersHandlerTestsFixture SeedDb()
        {
            _dbContext.Providers.AddRange(Providers);
            _dbContext.SaveChanges();

            return this;
        }

        public Task<GetAllProvidersQueryResult> Handle()
        {
            return _handler.Handle(new GetAllProvidersQuery(), CancellationToken.None);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}