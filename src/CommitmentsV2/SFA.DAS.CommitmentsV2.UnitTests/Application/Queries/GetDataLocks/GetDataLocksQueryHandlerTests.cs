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
using SFA.DAS.CommitmentsV2.Types;

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
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
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
            Assert.That(result.DataLocks.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WhenExpired_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().ExpireTheDataLockRecords();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.That(result.DataLocks.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WhenEventStatusRemoved_ThenShouldNotBeReturned()
        {
            await _fixture.SeedData().SetEventStatusRemoved();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.That(result.DataLocks.Count, Is.EqualTo(0));
        }

        public class GetDataLocksQueryHandlerTestsFixture : IDisposable
        {
            private readonly GetDataLocksQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDataLocksQuery _request;
            private GetDataLocksQueryResult _result;
            private readonly IFixture _autofixture;
            private List<DataLockStatus> _dataLocks;
            private long _apprenticeshipId;

            public GetDataLocksQueryHandlerTestsFixture()
            {
                _autofixture = new Fixture().Customize(new IgnoreVirtualMembersCustomisation());
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
                _autofixture.Customizations.Add(new ModelSpecimenBuilder());
                var apprenticeship = _autofixture.Create<Apprenticeship>();
                _apprenticeshipId = apprenticeship.Id;
                _dataLocks = _autofixture
                    .Build<DataLockStatus>()
                    .With(x => x.ApprenticeshipId, _apprenticeshipId)
                    .With(x => x.IsExpired, false)
                    .With(x => x.EventStatus, EventStatus.New)
                    .CreateMany(3)
                    .ToList();

                _dataLocks.ForEach(z => { z.ApprenticeshipId = _apprenticeshipId; z.IsExpired = false; z.EventStatus = EventStatus.New; });
                _db.DataLocks.AddRange(_dataLocks);

                var apprenticeship2 = _autofixture
                    .Build<Apprenticeship>()
                    .With(x => x.Id, ++count)
                    .Create();

                var additionalRecord = _autofixture
                    .Build<DataLockStatus>()
                    .With(x => x.ApprenticeshipId, apprenticeship2.Id)
                    .With(x => x.Apprenticeship, apprenticeship2)
                    .With(x => x.IsExpired, false)
                    .With(x => x.EventStatus, EventStatus.New)
                    .Create();
               
                _db.DataLocks.Add(additionalRecord);

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
                Assert.That(_result.DataLocks.Count, Is.EqualTo(resultCount));

                foreach (var result in _result.DataLocks)
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
