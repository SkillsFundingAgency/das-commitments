using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProvider
{
    [TestFixture]
    [Parallelizable]
    public class GetProviderQueryHandlerTests
    {
        private GetProviderQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetProviderQueryHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenProviderDoesExist_ThenShouldReturnResult()
        {
            var result = await _fixture.SetProvider().Handle(); 
            
            Assert.IsNotNull(result);
            Assert.AreEqual(_fixture.Provider.UkPrn, result.ProviderId);
            Assert.AreEqual(_fixture.Provider.Name, result.Name);
        }

        [Test]
        public async Task Handle_WhenProviderDoesNotExist_ThenShouldReturnNull()
        {
            var result = await _fixture.Handle(); 
            
            Assert.IsNull(result);
        }
    }

    public class GetProviderQueryHandlerTestsFixture
    {
        public GetProviderQuery Query { get; set; }
        public Provider Provider { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<GetProviderQuery, GetProviderQueryResult> Handler { get; set; }

        public GetProviderQueryHandlerTestsFixture()
        {
            Query = new GetProviderQuery(1);
            Provider = new Provider(1, "Foo", DateTime.UtcNow, DateTime.UtcNow);
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetProviderQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
            
            Db.Providers.Add(new Provider(2, "Bar", DateTime.UtcNow, DateTime.UtcNow));
            Db.SaveChanges();
        }

        public Task<GetProviderQueryResult> Handle()
        {
            return Handler.Handle(Query, CancellationToken.None);
        }

        public GetProviderQueryHandlerTestsFixture SetProvider()
        {
            Db.Providers.Add(Provider);
            Db.SaveChanges();
            
            return this;
        }
    }
}