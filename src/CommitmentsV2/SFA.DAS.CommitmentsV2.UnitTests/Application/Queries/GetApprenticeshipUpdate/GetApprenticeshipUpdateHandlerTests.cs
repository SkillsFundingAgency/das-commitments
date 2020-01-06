using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdate
{
    [TestFixture]
    public class GetApprenticeshipUpdateHandlerTests
    {
        private GetApprenticeshipUpdateHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetApprenticeshipUpdateHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithValueForApprenticeshipUpdate()
        {
            _fixture.SeedData();
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithNullForApprenticeshipUpdate()
        {
            _fixture.SeedData().WithNoMatchingApprenticeshipUpdates();
            var result = await _fixture.Handle();
            Assert.IsNotNull(result);
            Assert.IsNull(result.PendingApprenticeshipUpdate);
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithTheFirstApprenticeshipUpdate()
        {
            _fixture.SeedData(2);
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        public class GetApprenticeshipUpdateHandlerTestsFixture
        {
            private readonly GetApprenticeshipUpdateQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetApprenticeshipUpdateQuery _request;
            private GetApprenticeshipUpdateQueryResult _result;
            private readonly Fixture _autoFixture;
            private ApprenticeshipUpdate _apprenticeshipUpdate;
            private readonly long _apprenticeshipId;

            public GetApprenticeshipUpdateHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _apprenticeshipId = 1;
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                _handler = new GetApprenticeshipUpdateQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetApprenticeshipUpdateQueryResult> Handle()
            {
                _result = await _handler.Handle(_request, new CancellationToken());
                return _result;
            }

            public GetApprenticeshipUpdateHandlerTestsFixture SeedData(short count = 1)
            {
                _apprenticeshipUpdate = _autoFixture.Build<ApprenticeshipUpdate>().With(a => a.ApprenticeshipId, 1).Create();
                _db.ApprenticeshipUpdates.Add(_apprenticeshipUpdate);

                for (short i = 2; i <= count; i++){
                    var additionalRecord = _autoFixture.Build<ApprenticeshipUpdate>().With(a => a.ApprenticeshipId, i).Create();
                    _db.ApprenticeshipUpdates.Add(additionalRecord);
                }
                _db.SaveChanges();
                return this;
            }
            public GetApprenticeshipUpdateHandlerTestsFixture WithNoMatchingApprenticeshipUpdates()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId + 100);
                return this;
            }

            public void VerifyResultMapping()
            {
                AssertEquality(_apprenticeshipUpdate, _result.PendingApprenticeshipUpdate);
            }
        }

        private static void AssertEquality(ApprenticeshipUpdate source, GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate result)
        {
            Assert.AreEqual(source.Id, result.Id);

            Assert.AreEqual(source.ApprenticeshipId, result.ApprenticeshipId);
            Assert.AreEqual(source.Originator, result.Originator);
            Assert.AreEqual(source.FirstName, result.FirstName);
            Assert.AreEqual(source.LastName, result.LastName);
            Assert.AreEqual(source.TrainingType, result.TrainingType);
            Assert.AreEqual(source.TrainingCode, result.TrainingCode);
            Assert.AreEqual(source.TrainingName, result.TrainingName);
            Assert.AreEqual(source.Cost, result.Cost);
            Assert.AreEqual(source.StartDate, result.StartDate);
            Assert.AreEqual(source.EndDate, result.EndDate);
            Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
        }
    }
}
