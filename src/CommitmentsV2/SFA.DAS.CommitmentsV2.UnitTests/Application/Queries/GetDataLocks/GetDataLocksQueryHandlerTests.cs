using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDataLocks
{
    [TestFixture]
    public class GetDataLocksQueryHandlerTests
    {
        private GetDataLocksQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetDataLocksQueryHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithValues()
        {
            _fixture.SeedData();
            await _fixture.Handle();
            _fixture.VerifyResultMapping(3);
        }

        [Test]
        public async Task Handle_ThenShouldReturnAnEmptyArray()
        {
            _fixture.SeedData().WithNoMatchingApprenticeship();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DataLocks.Count);
        }

        [Test]
        public async Task Handle_WhenExpired_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().ExpireTheDataLockRecords();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DataLocks.Count);
        }

        [Test]
        public async Task Handle_WhenEventStatusRemoved_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().SetEventStatusRemoved();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DataLocks.Count);
        }

        public class GetDataLocksQueryHandlerTestsFixture
        {
            private readonly GetDataLocksQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDataLocksQuery _request;
            private GetDataLocksQueryResult _result;
            private readonly Fixture _autofixture;
            private List<DataLockStatus> _dataLocks;
            private readonly long _apprenticeshipId;

            public GetDataLocksQueryHandlerTestsFixture()
            {
                _autofixture = new Fixture();

                _apprenticeshipId = 1;
                _request = new GetDataLocksQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging().Options);
                _handler = new GetDataLocksQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetDataLocksQueryResult> Handle()
            {
                _result = await _handler.Handle(_request, new CancellationToken());
                return _result;
            }

            public GetDataLocksQueryHandlerTestsFixture SeedData(short count = 1)
            {
                _autofixture.Customizations.Add(new DataLockStatusSpecimenBuilder());
                _dataLocks = _autofixture.Create<List<DataLockStatus>>();
                _dataLocks.ForEach(z => { z.ApprenticeshipId = _apprenticeshipId; z.IsExpired = false; z.EventStatus = Types.EventStatus.New; z.Apprenticeship.Id = z.ApprenticeshipId; });
                _db.DataLocks.AddRange(_dataLocks);

                var additionalRecord = _autofixture.Create<List<DataLockStatus>>();
                additionalRecord.ForEach(z => { z.ApprenticeshipId = ++count; z.Apprenticeship.Id = z.ApprenticeshipId; z.IsExpired = false; z.EventStatus = Types.EventStatus.New; });
                _db.DataLocks.AddRange(additionalRecord);

                _db.SaveChanges();
                return this;
            }

            public GetDataLocksQueryHandlerTestsFixture WithNoMatchingApprenticeship()
            {
                _request = new GetDataLocksQuery(_apprenticeshipId + 100);
                return this;
            }

            internal async Task<GetDataLocksQueryHandlerTestsFixture> ExpireTheDataLockRecords()
            {
                await _db.DataLocks.Where(x => x.ApprenticeshipId == _apprenticeshipId).ForEachAsync(x => x.IsExpired = true);
                _db.SaveChanges();
                return this;
            }

            internal async Task<GetDataLocksQueryHandlerTestsFixture> SetEventStatusRemoved()
            {
                await _db.DataLocks.Where(x => x.ApprenticeshipId == _apprenticeshipId).ForEachAsync(x => x.EventStatus = Types.EventStatus.Removed);
                _db.SaveChanges();
                return this;
            }

            public void VerifyResultMapping(int resultCount)
            {
                Assert.AreEqual(resultCount, _result.DataLocks.Count);

                foreach (var result in _result.DataLocks)
                {
                    AssertEquality(_dataLocks.Single(x => x.Id == result.Id), result);
                }
            }

            private static void AssertEquality(DataLockStatus source, GetDataLocksQueryResult.DataLock result)
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

        public class DataLockStatusSpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request,
                ISpecimenContext context)
            {
                var pi = request as Type;

                if (pi == null)
                {
                    return new NoSpecimen();
                }
                if (pi == typeof(ApprenticeshipBase)
                    || pi.Name == "Apprenticeship")
                {
                    return new Apprenticeship();
                }

                if (pi == typeof(ApprenticeshipUpdate))
                {
                    return new ApprenticeshipUpdate();
                }

                return new NoSpecimen();
            }
        }
    }
}
