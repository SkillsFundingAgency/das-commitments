using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfPartyRequest
{
    [TestFixture]
    public class GetChangeOfPartyRequestQueryHandlerTests
    {
        private GetChangeOfPartyRequestQueryHandlerTestsFixture _fixture;

        

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetChangeOfPartyRequestQueryHandlerTestsFixture();
        }

        [Test]
        public async Task Then_ShouldReturnResult()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        private class GetChangeOfPartyRequestQueryHandlerTestsFixture
        {
            private Fixture _autoFixture;

            private readonly ProviderCommitmentsDbContext _db;

            public long ChangeOfPartyRequestId { get; set; }
            public long ApprenticeshipId{ get; set; }
            private GetChangeOfPartyRequestQuery _query;
            private GetChangeOfPartyRequestQueryHandler _handler;
            private GetChangeOfPartyRequestQueryResult _result;

            public GetChangeOfPartyRequestQueryHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                ChangeOfPartyRequestId = _autoFixture.Create<long>();
                ApprenticeshipId = _autoFixture.Create<long>();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();

                _query = new GetChangeOfPartyRequestQuery(ChangeOfPartyRequestId);

                _handler = new GetChangeOfPartyRequestQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db)); 
            }

            public async Task Handle()
            {
                _result = await _handler.Handle(_query, new CancellationToken());
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(ApprenticeshipId, _result.ApprenticeshipId);
            }

            private void SeedData()
            {
                var apprenticeshipId = _autoFixture.Create<long>();

                var request = new ChangeOfPartyRequest();
                request.SetValue(x => x.Id, ChangeOfPartyRequestId);
                request.SetValue(x => x.ApprenticeshipId, apprenticeshipId);

                _db.ChangeOfPartyRequests.Add(request);
            }
        }
    }
}
