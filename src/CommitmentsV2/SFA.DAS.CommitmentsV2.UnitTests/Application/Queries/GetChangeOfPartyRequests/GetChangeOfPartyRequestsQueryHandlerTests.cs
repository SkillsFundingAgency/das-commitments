﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfPartyRequests
{
    [TestFixture]
    public class GetChangeOfPartyRequestsQueryHandlerTests
    {
        private ChangeOfPartyRequestsQueryHandlersTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestsQueryHandlersTestsFixture();
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

        private class ChangeOfPartyRequestsQueryHandlersTestsFixture : IDisposable
        {
            private readonly GetChangeOfPartyRequestsQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetChangeOfPartyRequestsQuery _request;
            private GetChangeOfPartyRequestsQueryResult _result;
            private readonly Fixture _autoFixture;
            private List<ChangeOfPartyRequest> _changeOfPartyRequests;
            private readonly long _apprenticeshipId;
            private readonly string _employerName;

            public ChangeOfPartyRequestsQueryHandlersTestsFixture()
            {
                _autoFixture = new Fixture();

                _employerName = _autoFixture.Create<string>();
                _apprenticeshipId = _autoFixture.Create<long>();
                _request = new GetChangeOfPartyRequestsQuery(_apprenticeshipId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();
                _handler = new GetChangeOfPartyRequestsQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetChangeOfPartyRequestsQueryResult> Handle()
            {
                _result = await _handler.Handle(TestHelper.Clone(_request), new CancellationToken());
                return _result;
            }

            private void SeedData()
            {
                var employer = new AccountLegalEntity();
                employer.SetValue(x => x.Name, _employerName);

                _changeOfPartyRequests = new List<ChangeOfPartyRequest>();

                for (var i = 1; i <= 10; i++)
                {
                    var request = new ChangeOfPartyRequest();
                    request.SetValue(x => x.Id, i);
                    request.SetValue(x => x.ApprenticeshipId, _apprenticeshipId);
                    request.SetValue(x => x.ChangeOfPartyType, _autoFixture.Create<ChangeOfPartyRequestType>());
                    request.SetValue(x => x.OriginatingParty, _autoFixture.Create<Party>());
                    request.SetValue(x => x.Status, _autoFixture.Create<ChangeOfPartyRequestStatus>());
                    request.SetValue(x => x.AccountLegalEntity, employer);
                    request.SetValue(x => x.StartDate, _autoFixture.Create<DateTime>());
                    request.SetValue(x => x.EndDate, _autoFixture.Create<DateTime>());
                    request.SetValue(x => x.ProviderId, _autoFixture.Create<long?>());
                    _changeOfPartyRequests.Add(request);
                }

                _db.AccountLegalEntities.Add(employer);
                _db.ChangeOfPartyRequests.AddRange(_changeOfPartyRequests);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_changeOfPartyRequests.Count(), _result.ChangeOfPartyRequests.Count);

                foreach (var sourceItem in _changeOfPartyRequests)
                {
                    var resultItem = _result.ChangeOfPartyRequests.Single(x => x.Id == sourceItem.Id);
                    AssertEquality(sourceItem, resultItem);
                    Assert.AreEqual(_employerName, resultItem.EmployerName);
                }
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }

        private static void AssertEquality(ChangeOfPartyRequest source, GetChangeOfPartyRequestsQueryResult.ChangeOfPartyRequest result)
        {
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.ChangeOfPartyType, result.ChangeOfPartyType);
            Assert.AreEqual(source.OriginatingParty, result.OriginatingParty);
            Assert.AreEqual(source.Status, result.Status);
            Assert.AreEqual(source.StartDate, result.StartDate);
            Assert.AreEqual(source.EndDate, result.EndDate);
            Assert.AreEqual(source.Price, result.Price);
            Assert.AreEqual(source.NewApprenticeshipId, result.NewApprenticeshipId);
            Assert.AreEqual(source.ProviderId, result.ProviderId);
        }
    }
}
