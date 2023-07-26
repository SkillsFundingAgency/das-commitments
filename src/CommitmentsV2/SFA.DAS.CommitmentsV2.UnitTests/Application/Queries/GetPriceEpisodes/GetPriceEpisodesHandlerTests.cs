using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetPriceEpisodes
{
    [TestFixture]
    public class GetPriceEpisodesHandlerTests
    {
        private GetPriceEpisodesHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetPriceEpisodesHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResult()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        public class GetPriceEpisodesHandlerTestsFixture : IDisposable
        {
            private readonly GetPriceEpisodesQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetPriceEpisodesQuery _request;
            private GetPriceEpisodesQueryResult _result;
            private readonly Fixture _autoFixture;
            private List<PriceHistory> _priceEpisodes;
            private readonly long _apprenticeshipId;

            public GetPriceEpisodesHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _apprenticeshipId = _autoFixture.Create<long>();
                _request = new GetPriceEpisodesQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();
                _handler = new GetPriceEpisodesQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }


            public async Task<GetPriceEpisodesQueryResult> Handle()
            {
                _result = await _handler.Handle(TestHelper.Clone(_request), new CancellationToken());
                return _result;
            }

            private void SeedData()
            {
                _priceEpisodes = new List<PriceHistory>();

                for (var i = 0; i < 10; i++)
                {
                    var priceEpisode = new PriceHistory
                    {
                        Id = i+1,
                        ApprenticeshipId = _apprenticeshipId
                    };

                    _priceEpisodes.Add(priceEpisode);
                }

                _db.PriceHistory.AddRange(_priceEpisodes);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_priceEpisodes.Count(), _result.PriceEpisodes.Count);

                foreach (var sourceItem in _priceEpisodes)
                {
                    AssertEquality(sourceItem, _result.PriceEpisodes.Single(x => x.Id == sourceItem.Id));
                }
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }

        private static void AssertEquality(PriceHistory source, GetPriceEpisodesQueryResult.PriceEpisode result)
        {
            Assert.AreEqual(source.Id, result.Id);

            Assert.AreEqual(source.ApprenticeshipId, result.ApprenticeshipId);
            Assert.AreEqual(source.FromDate, result.FromDate);
            Assert.AreEqual(source.ToDate, result.ToDate);
            Assert.AreEqual(source.Cost, result.Cost);
        }
    }
}
