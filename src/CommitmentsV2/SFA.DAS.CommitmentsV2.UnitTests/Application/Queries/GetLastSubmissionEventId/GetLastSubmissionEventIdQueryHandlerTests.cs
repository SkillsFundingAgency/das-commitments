﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetLastSubmissionEventId
{
    [TestFixture]
    public class GetLastSubmissionEventIdQueryHandlerTests
    {
        [Test]
        public async Task Get_LastSubmission_EventId_Is_Returned_Correctly()
        {
            var fixture = new GetLastSubmissionEventIdQueryHandlerTestsFixture();
            var result = await fixture.Handle();

            Assert.AreEqual(fixture.AddEpaLastSubmissionEventId, result.Value);
        }

        public class GetLastSubmissionEventIdQueryHandlerTestsFixture
        {
            public long? AddEpaLastSubmissionEventId;
            private ProviderCommitmentsDbContext _db { get; set; }
            private GetLastSubmissionEventIdQueryHandler _sut { get; set; }

            private GetLastSubmissionEventIdQuery _query;

            public GetLastSubmissionEventIdQueryHandlerTestsFixture()
            {
                AddEpaLastSubmissionEventId = 10;

                _query = new GetLastSubmissionEventIdQuery();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

               
                _sut = new GetLastSubmissionEventIdQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<GetLastSubmissionEventIdQueryHandler>>());
                SeedData();
            }

            public void SeedData()
            {
                _db.JobProgress.Add(new CommitmentsV2.Models.JobProgress { AddEpaLastSubmissionEventId = AddEpaLastSubmissionEventId, Lock = "X" });
               
                _db.SaveChanges();
            }

            public async Task<long?> Handle()
            {
                return await _sut.Handle(_query, CancellationToken.None);
            }
        }
    }
}