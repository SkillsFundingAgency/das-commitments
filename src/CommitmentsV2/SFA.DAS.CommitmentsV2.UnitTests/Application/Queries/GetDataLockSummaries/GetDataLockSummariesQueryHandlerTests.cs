using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDataLockSummaries
{
    [TestFixture]
    public class GetDataLockSummariesQueryHandlerTests
    {
        private GetDataLockSummariesQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetDataLockSummariesQueryHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithValues()
        {
            _fixture.SeedData();
            var result = await _fixture.Handle();
            _fixture.VerifyResultMapping(1, result.DataLocksWithCourseMismatch);
            _fixture.VerifyResultMapping(2, result.DataLocksWithOnlyPriceMismatch);
        }

        [Test]
        public async Task Handle_ThenShouldReturnAnEmptyArray()
        {
            _fixture.SeedData().WithNoMatchingApprenticeship();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.That(result.DataLocksWithCourseMismatch.Count, Is.EqualTo(0));
            Assert.That(result.DataLocksWithOnlyPriceMismatch.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WhenExpired_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().ExpireTheDataLockRecords();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.That(result.DataLocksWithCourseMismatch.Count, Is.EqualTo(0));
            Assert.That(result.DataLocksWithOnlyPriceMismatch.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WhenEventStatusRemoved_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().SetEventStatusRemoved();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.That(result.DataLocksWithCourseMismatch.Count, Is.EqualTo(0));
            Assert.That(result.DataLocksWithOnlyPriceMismatch.Count, Is.EqualTo(0));
        }

        public class GetDataLockSummariesQueryHandlerTestsFixture : IDisposable
        {
            private readonly GetDataLockSummariesQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDataLockSummariesQuery _request;
            private GetDataLockSummariesQueryResult _result;
            private readonly IFixture _autofixture;
            private List<DataLockStatus> _dataLocks;
            private readonly long _apprenticeshipId;

            public GetDataLockSummariesQueryHandlerTestsFixture()
            {
                _autofixture = new Fixture().Customize(new IgnoreVirtualMembersCustomisation());

                _apprenticeshipId = 1;
                _request = new GetDataLockSummariesQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging().Options);
                _handler = new GetDataLockSummariesQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetDataLockSummariesQueryResult> Handle()
            {
                _result = await _handler.Handle(_request, new CancellationToken());
                return _result;
            }

            public GetDataLockSummariesQueryHandlerTestsFixture SeedData(short count = 1)
            {
                _autofixture.Customizations.Add(new ModelSpecimenBuilder());

                _dataLocks = _autofixture.CreateMany<DataLockStatus>(6).ToList();
                // first 3 are for the known apprenticeship id
                _dataLocks.Take(3).ToList().ForEach(d => { d.ApprenticeshipId = _apprenticeshipId; });

                // first and second are is price only datalocks
                _dataLocks.Take(2).ToList().ForEach(d => { d.ErrorCode = DataLockErrorCode.Dlock07; });
                
                // third is with course error datalock
                _dataLocks.Skip(2).Take(1).ToList().ForEach(d => { d.ErrorCode = DataLockErrorCode.Dlock03; });
                
                // last 3 are not for the known apprenticeship id
                _dataLocks.Skip(3).Take(3).ToList().ForEach(d => { d.ApprenticeshipId = ++count; });
                
                // all are unhandled new data locks
                _dataLocks.ToList().ForEach(d => { d.ApprenticeshipId = d.ApprenticeshipId; d.IsExpired = false; d.EventStatus = EventStatus.New; d.IsResolved = false; d.Status = Status.Unknown; });
                
                _db.DataLocks.AddRange(_dataLocks);
                _db.SaveChanges();
                return this;
            }

            

            public GetDataLockSummariesQueryHandlerTestsFixture WithNoMatchingApprenticeship()
            {
                _request = new GetDataLockSummariesQuery(_apprenticeshipId + 100);
                return this;
            }

            internal async Task<GetDataLockSummariesQueryHandlerTestsFixture> ExpireTheDataLockRecords()
            {
                await _db.DataLocks.Where(x => x.ApprenticeshipId == _apprenticeshipId).ForEachAsync(x => x.IsExpired = true);
                _db.SaveChanges();
                return this;
            }

            internal async Task<GetDataLockSummariesQueryHandlerTestsFixture> SetEventStatusRemoved()
            {
                await _db.DataLocks.Where(x => x.ApprenticeshipId == _apprenticeshipId).ForEachAsync(x => x.EventStatus = Types.EventStatus.Removed);
                _db.SaveChanges();
                return this;
            }

            public void VerifyResultMapping(int resultCount, IReadOnlyCollection<DataLock> resultDataLocks)
            {
                Assert.That(resultDataLocks.Count, Is.EqualTo(resultCount));

                foreach (var result in resultDataLocks)
                {
                    AssertEquality(_dataLocks.Single(x => x.Id == result.Id), result);
                }
            }

            private static void AssertEquality(DataLockStatus source, DataLock result)
            {
                Assert.That(result.Id, Is.EqualTo(source.Id));
                Assert.That(result.DataLockEventDatetime, Is.EqualTo(source.DataLockEventDatetime));
                Assert.That(result.PriceEpisodeIdentifier, Is.EqualTo(source.PriceEpisodeIdentifier));
                Assert.That(result.ApprenticeshipId, Is.EqualTo(source.ApprenticeshipId));
                Assert.That(result.IlrTrainingCourseCode, Is.EqualTo(source.IlrTrainingCourseCode));
                Assert.That(result.IlrActualStartDate, Is.EqualTo(source.IlrActualStartDate));
                Assert.That(result.IlrEffectiveFromDate, Is.EqualTo(source.IlrEffectiveFromDate));
                Assert.That(result.IlrPriceEffectiveToDate, Is.EqualTo(source.IlrPriceEffectiveToDate));
                Assert.That(result.IlrTotalCost, Is.EqualTo(source.IlrTotalCost));
                Assert.That(result.ErrorCode, Is.EqualTo(source.ErrorCode));
                Assert.That(result.DataLockStatus, Is.EqualTo(source.Status));
                Assert.That(result.TriageStatus, Is.EqualTo(source.TriageStatus));
                Assert.That(result.IsResolved, Is.EqualTo(source.IsResolved));
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }
    }
}
