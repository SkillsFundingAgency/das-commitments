using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetLastSubmissionEventId
{
    [TestFixture]
    public class GetLastSubmissionEventIdQueryHandlerTests
    {
        [Test]
        public async Task Get_LastSubmission_EventId_Is_Returned_Correctly()
        {
            using var fixture = new GetLastSubmissionEventIdQueryHandlerTestsFixture();
            var result = await fixture.Handle();

            Assert.That(result.Value, Is.EqualTo(fixture.AddEpaLastSubmissionEventId));
        }

        private class GetLastSubmissionEventIdQueryHandlerTestsFixture : IDisposable
        {
            public readonly long? AddEpaLastSubmissionEventId;
            private ProviderCommitmentsDbContext _db { get; set; }
            private GetLastSubmissionEventIdQueryHandler _sut { get; set; }

            private GetLastSubmissionEventIdQuery _query;

            public GetLastSubmissionEventIdQueryHandlerTestsFixture()
            {
                AddEpaLastSubmissionEventId = 10;

                _query = new GetLastSubmissionEventIdQuery();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

               
                _sut = new GetLastSubmissionEventIdQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<GetLastSubmissionEventIdQueryHandler>>());
                SeedData();
            }

            private void SeedData()
            {
                _db.JobProgress.Add(new CommitmentsV2.Models.JobProgress { AddEpaLastSubmissionEventId = AddEpaLastSubmissionEventId, Lock = "X" });
               
                _db.SaveChanges();
            }

            public async Task<long?> Handle()
            {
                return await _sut.Handle(_query, CancellationToken.None);
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
