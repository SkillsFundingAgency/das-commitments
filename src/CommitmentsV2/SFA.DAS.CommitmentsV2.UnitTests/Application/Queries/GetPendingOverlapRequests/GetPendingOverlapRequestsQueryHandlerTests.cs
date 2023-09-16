using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using OverlappingTrainingDateRequest = SFA.DAS.CommitmentsV2.Models.OverlappingTrainingDateRequest;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetPendingOverlapRequests
{
    [TestFixture]
    public class GetPendingOverlapRequestsQueryHandlerTests
    {
        private GetPendingOverlapRequestsQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetPendingOverlapRequestsQueryHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_ThenShouldOnlyGetPendingRequests()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        public class GetPendingOverlapRequestsQueryHandlerTestsFixture : IDisposable
        {
            private readonly GetPendingOverlapRequestsQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetPendingOverlapRequestsQuery _request;
            private GetPendingOverlapRequestsQueryResult _result;
            private readonly Fixture _autoFixture;
            private OverlappingTrainingDateRequest _testOverlappingTrainingDateRequest;
            private long _draftApprenticeshipId;

            public GetPendingOverlapRequestsQueryHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _draftApprenticeshipId = _autoFixture.Create<long>();
                _request = new GetPendingOverlapRequestsQuery(_draftApprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                
                SeedData();
                
                _handler = new GetPendingOverlapRequestsQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetPendingOverlapRequestsQueryResult> Handle()
            {
                _result = await _handler.Handle(TestHelper.Clone(_request), new CancellationToken());
                return _result;
            }

            private void SeedData()
            {
                var seedData = _autoFixture
                .CreateMany<OverlappingTrainingDateRequest>();

                foreach (var item in seedData)
                {
                    item.SetValue(x => x.Id, _autoFixture.Create<long?>());
                    item.SetValue(x => x.CreatedOn, DateTime.Now.AddDays(-7));
                    item.SetValue(x => x.PreviousApprenticeshipId, _autoFixture.Create<long?>());
                    item.SetValue(x => x.ResolutionType, _autoFixture.Create<OverlappingTrainingDateRequestResolutionType>());
                    item.SetValue(x => x.Status, OverlappingTrainingDateRequestStatus.Pending);
                    item.SetValue(x => x.ActionedOn, _autoFixture.Create<DateTime>());
                    item.SetValue(x => x.DraftApprenticeshipId, _autoFixture.Create<long>());
                }

                var testData = _autoFixture
                .CreateMany<OverlappingTrainingDateRequest>();

                foreach (var item in testData)
                {
                    item.SetValue(x => x.Id, _autoFixture.Create<long?>());
                    item.SetValue(x => x.CreatedOn, DateTime.Now.AddDays(-7));
                    item.SetValue(x => x.PreviousApprenticeshipId, _autoFixture.Create<long?>());
                    item.SetValue(x => x.ResolutionType, _autoFixture.Create<OverlappingTrainingDateRequestResolutionType>());
                    item.SetValue(x => x.Status, _autoFixture.Create<Generator<OverlappingTrainingDateRequestStatus>>().Where(g => g != OverlappingTrainingDateRequestStatus.Pending).First());
                    item.SetValue(x => x.ActionedOn, _autoFixture.Create<DateTime>());
                    item.SetValue(x => x.DraftApprenticeshipId, _draftApprenticeshipId);
                }

                _testOverlappingTrainingDateRequest = testData.First();
                _testOverlappingTrainingDateRequest.SetValue(x => x.Status, OverlappingTrainingDateRequestStatus.Pending);

                _db.OverlappingTrainingDateRequests.AddRange(seedData.Concat(testData));
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                _result.CreatedOn.Should().Be(_testOverlappingTrainingDateRequest.CreatedOn);
                _result.DraftApprenticeshipId.Should().Be(_testOverlappingTrainingDateRequest.DraftApprenticeshipId);
                _result.PreviousApprenticeshipId.Should().Be(_testOverlappingTrainingDateRequest.PreviousApprenticeshipId);
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }
    }
}