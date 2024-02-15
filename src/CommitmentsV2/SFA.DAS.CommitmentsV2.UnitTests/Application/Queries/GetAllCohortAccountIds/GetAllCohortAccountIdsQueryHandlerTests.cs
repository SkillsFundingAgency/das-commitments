using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllCohortAccountIds;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllCohortAccountIds
{
    [TestFixture]
    [Parallelizable]
    public class GetAllCohortAccountIdsQueryHandlerTests
    {
        [Test]
        public async Task Handle_WhenRequested_ThenShouldReturnCorrectReferenceForRequester()
        {
            using var fixture = new GetAllCohortAccountIdsQueryHandlerTestFixtures()
                .CreateCommitments();

            var result = await fixture.Handle();

            result.Should().NotBeNull();
            result.AccountIds.Count().Should().Be(2);
            result.AccountIds[0].Should().Be(1);
            result.AccountIds[1].Should().Be(2);
        }
    }

    public class GetAllCohortAccountIdsQueryHandlerTestFixtures : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IAuthenticationService> AuthenticationServiceMock { get; set; }
        public GetAllCohortAccountIdsQueryHandler Handler { get; set; }

        public GetAllCohortAccountIdsQueryHandlerTestFixtures()
        {
            AuthenticationServiceMock = new Mock<IAuthenticationService>();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            Handler = new GetAllCohortAccountIdsQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), Mock.Of<ILogger<GetAllCohortAccountIdsQueryHandler>>());
        }

        public Task<GetAllCohortAccountIdsQueryResult> Handle()
        {
            var query = new GetAllCohortAccountIdsQuery();
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetAllCohortAccountIdsQueryHandlerTestFixtures CreateCommitments()
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var commitments = new List<Cohort>
            {
                new Cohort(
                    autoFixture.Create<long>(),
                    1,
                    autoFixture.Create<long>(),
                    Party.Employer,
                    new UserInfo()),

                new Cohort(
                    autoFixture.Create<long>(),
                    1,
                    autoFixture.Create<long>(),
                    Party.Employer,
                    new UserInfo()),

                new Cohort(
                    autoFixture.Create<long>(),
                    2,
                    autoFixture.Create<long>(),
                    Party.Employer,
                    new UserInfo()),
            };

            Db.Cohorts.AddRange(commitments);
            Db.SaveChanges();

            return this;
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}