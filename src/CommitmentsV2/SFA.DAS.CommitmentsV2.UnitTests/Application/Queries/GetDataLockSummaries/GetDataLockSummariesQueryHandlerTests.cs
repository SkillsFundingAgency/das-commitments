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
            Assert.AreEqual(0, result.DataLocksWithCourseMismatch.Count);
            Assert.AreEqual(0, result.DataLocksWithOnlyPriceMismatch.Count);
        }

        [Test]
        public async Task Handle_WhenExpired_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().ExpireTheDataLockRecords();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DataLocksWithCourseMismatch.Count);
            Assert.AreEqual(0, result.DataLocksWithOnlyPriceMismatch.Count);
        }

        [Test]
        public async Task Handle_WhenEventStatusRemoved_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().SetEventStatusRemoved();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DataLocksWithCourseMismatch.Count);
            Assert.AreEqual(0, result.DataLocksWithOnlyPriceMismatch.Count);
        }

        public class GetDataLockSummariesQueryHandlerTestsFixture
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
                Assert.AreEqual(resultCount, resultDataLocks.Count);

                foreach (var result in resultDataLocks)
                {
                    AssertEquality(_dataLocks.Single(x => x.Id == result.Id), result);
                }
            }

            private static void AssertEquality(DataLockStatus source, DataLock result)
            {
                Assert.AreEqual(source.Id, result.Id);
                Assert.AreEqual(source.DataLockEventDatetime, result.DataLockEventDatetime);
                Assert.AreEqual(source.PriceEpisodeIdentifier, result.PriceEpisodeIdentifier);
                Assert.AreEqual(source.ApprenticeshipId, result.ApprenticeshipId);
                Assert.AreEqual(source.IlrTrainingCourseCode, result.IlrTrainingCourseCode);
                Assert.AreEqual(source.IlrActualStartDate, result.IlrActualStartDate);
                Assert.AreEqual(source.IlrEffectiveFromDate, result.IlrEffectiveFromDate);
                Assert.AreEqual(source.IlrPriceEffectiveToDate, result.IlrPriceEffectiveToDate);
                Assert.AreEqual(source.IlrTotalCost, result.IlrTotalCost);
                Assert.AreEqual(source.ErrorCode, result.ErrorCode);
                Assert.AreEqual(source.Status, result.DataLockStatus);
                Assert.AreEqual(source.TriageStatus, result.TriageStatus);
                Assert.AreEqual(source.IsResolved, result.IsResolved);
            }
        }
    }
}
