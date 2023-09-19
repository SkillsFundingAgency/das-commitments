using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

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

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
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
            }
        }

        private static void AssertEquality(OverlappingTrainingDateRequest source, GetOverlappingTrainingDateRequestQueryResult result)
        {
            Assert.AreEqual(1, result.OverlappingTrainingDateRequests.Count);

            Assert.AreEqual(source.Id, result.OverlappingTrainingDateRequests.First().Id);
            Assert.AreEqual(source.DraftApprenticeshipId, result.OverlappingTrainingDateRequests.First().DraftApprenticeshipId);
            Assert.AreEqual(source.PreviousApprenticeshipId, result.OverlappingTrainingDateRequests.First().PreviousApprenticeshipId);
            Assert.AreEqual(source.Status, result.OverlappingTrainingDateRequests.First().Status);
            Assert.AreEqual(source.ResolutionType, result.OverlappingTrainingDateRequests.First().ResolutionType);
            Assert.AreEqual(source.ActionedOn, result.OverlappingTrainingDateRequests.First().ActionedOn);
        }
    }
}