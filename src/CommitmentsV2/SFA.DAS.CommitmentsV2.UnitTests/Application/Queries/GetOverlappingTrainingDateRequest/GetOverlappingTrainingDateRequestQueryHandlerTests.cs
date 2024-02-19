using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetOverlappingTrainingDateRequest
{
    [TestFixture]
    public class GetOverlappingTrainingDateRequestQueryHandlerTests
    {
        private OverlappingTrainingDateRequestQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlappingTrainingDateRequestQueryHandlerTestsFixture();
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

        private class OverlappingTrainingDateRequestQueryHandlerTestsFixture : IDisposable
        {
            private readonly GetOverlappingTrainingDateRequestQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetOverlappingTrainingDateRequestQuery _request;
            private GetOverlappingTrainingDateRequestQueryResult _result;
            private readonly Fixture _autoFixture;

            private readonly long _apprenticeshipId;
            private readonly string _employerName;
            private OverlappingTrainingDateRequest _overlappingTrainingDateRequest;

            public OverlappingTrainingDateRequestQueryHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _employerName = _autoFixture.Create<string>();
                _apprenticeshipId = _autoFixture.Create<long>();
                _request = new GetOverlappingTrainingDateRequestQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
                SeedData();
                _handler = new GetOverlappingTrainingDateRequestQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetOverlappingTrainingDateRequestQueryResult> Handle()
            {
                _result = await _handler.Handle(TestHelper.Clone(_request), new CancellationToken());
                return _result;
            }

            private void SeedData()
            {
                var employer = new AccountLegalEntity();
                employer.SetValue(x => x.Name, _employerName);
                _overlappingTrainingDateRequest = new OverlappingTrainingDateRequest();
                _overlappingTrainingDateRequest.SetValue(x => x.Id, _autoFixture.Create<long?>());
                _overlappingTrainingDateRequest.SetValue(x => x.PreviousApprenticeshipId, _apprenticeshipId);
                _overlappingTrainingDateRequest.SetValue(x => x.DraftApprenticeshipId, _autoFixture.Create<long?>());
                _overlappingTrainingDateRequest.SetValue(x => x.ResolutionType, _autoFixture.Create<OverlappingTrainingDateRequestResolutionType>());
                _overlappingTrainingDateRequest.SetValue(x => x.Status, _autoFixture.Create<OverlappingTrainingDateRequestStatus>());
                _overlappingTrainingDateRequest.SetValue(x => x.ActionedOn, _autoFixture.Create<DateTime>());

                _db.OverlappingTrainingDateRequests.Add(_overlappingTrainingDateRequest);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                AssertEquality(_overlappingTrainingDateRequest, _result);
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private static void AssertEquality(OverlappingTrainingDateRequest source, GetOverlappingTrainingDateRequestQueryResult result)
        {
            Assert.That(result.OverlappingTrainingDateRequests, Has.Count.EqualTo(1));

            Assert.Multiple(() =>
            {
                Assert.That(result.OverlappingTrainingDateRequests.First().Id, Is.EqualTo(source.Id));
                Assert.That(result.OverlappingTrainingDateRequests.First().DraftApprenticeshipId, Is.EqualTo(source.DraftApprenticeshipId));
                Assert.That(result.OverlappingTrainingDateRequests.First().PreviousApprenticeshipId, Is.EqualTo(source.PreviousApprenticeshipId));
                Assert.That(result.OverlappingTrainingDateRequests.First().Status, Is.EqualTo(source.Status));
                Assert.That(result.OverlappingTrainingDateRequests.First().ResolutionType, Is.EqualTo(source.ResolutionType));
                Assert.That(result.OverlappingTrainingDateRequests.First().ActionedOn, Is.EqualTo(source.ActionedOn));
            });
        }
    }
}