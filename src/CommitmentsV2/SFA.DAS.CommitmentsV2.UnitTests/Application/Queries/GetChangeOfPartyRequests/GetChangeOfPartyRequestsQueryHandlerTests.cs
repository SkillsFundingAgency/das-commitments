using System;
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

        [Test]
        public async Task Handle_ThenShouldReturnResult()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        public class ChangeOfPartyRequestsQueryHandlersTestsFixture
        {
            private readonly GetChangeOfPartyRequestsQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetChangeOfPartyRequestsQuery _request;
            private GetChangeOfPartyRequestsQueryResult _result;
            private readonly Fixture _autoFixture;
            private List<ChangeOfPartyRequest> _changeOfPartyRequests;
            private readonly long _apprenticeshipId;

            public ChangeOfPartyRequestsQueryHandlersTestsFixture()
            {
                _autoFixture = new Fixture();

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
                _changeOfPartyRequests = new List<ChangeOfPartyRequest>();

                for (var i = 1; i <= 10; i++)
                {
                    var request = new ChangeOfPartyRequest();
                    request.SetValue("Id", i);
                    request.SetValue("ApprenticeshipId", _apprenticeshipId);
                    request.SetValue("ChangeOfPartyType", _autoFixture.Create<ChangeOfPartyRequestType>());
                    request.SetValue("OriginatingParty", _autoFixture.Create<Party>());
                    request.SetValue("Status", _autoFixture.Create<ChangeOfPartyRequestStatus>());
                    _changeOfPartyRequests.Add(request);
                }

                _db.ChangeOfPartyRequests.AddRange(_changeOfPartyRequests);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_changeOfPartyRequests.Count(), _result.ChangeOfPartyRequests.Count);

                foreach (var sourceItem in _changeOfPartyRequests)
                {
                    AssertEquality(sourceItem, _result.ChangeOfPartyRequests.Single(x => x.Id == sourceItem.Id));
                }
            }
        }

        private static void AssertEquality(ChangeOfPartyRequest source, GetChangeOfPartyRequestsQueryResult.ChangeOfPartyRequest result)
        {
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.ChangeOfPartyType, result.ChangeOfPartyType);
            Assert.AreEqual(source.OriginatingParty, result.OriginatingParty);
            Assert.AreEqual(source.Status, result.Status);
        }
    }
}
