﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedProviders
{
    [TestFixture]
    [Parallelizable]
    public class GetApprovedProvidersQueryHandlerTests
    {
        private GetApprovedProvidersQueryHandlerFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetApprovedProvidersQueryHandlerFixture();
        }

        [Test]
        public async Task Handle_WhenCohortApprovedByBoth_And_TransferSender_IdIsNull_ThenShouldReturnResult()
        {
            var result = await _fixture.AddApprovedCohortAndProviderForAccount()
                .AddApprovedCohortAndProviderForAccount()
                .AddNotApprovedCohortAndProviderForAccount()
                .SeedDb().Handle();

            Assert.AreEqual(2, result.ProviderIds.Count());
        }

        [Test]
        public async Task Handle_WhenCohortApprovedByBoth_And_TransferSenderNotApproved_ThenShouldNotReturnResult()
        {
            var result = await _fixture.AddCohortAndProvider_WithTransfserSenderNotApproved()
                .SeedDb().Handle();

            Assert.AreEqual(0, result.ProviderIds.Count());
        }

        [Test]
        public async Task Handle_WhenCohortApprovedByBoth_And_TransferSenderApproved_ThenShouldReturnResult()
        {
            var result = await _fixture.AddCohortAndProvider_WithTransferSenderApproved()
                .SeedDb().Handle();

            Assert.AreEqual(1, result.ProviderIds.Count());
        }

        [Test]
        public async Task Handle_WhenCohortNotFullyApproved_ThenShouldNotReturnResult()
        {
            var result = await _fixture.AddNotApprovedCohortAndProviderForAccount().SeedDb().Handle();

            Assert.AreEqual(0, result.ProviderIds.Count());
        }
    }

    public class GetApprovedProvidersQueryHandlerFixture
    {
        public GetApprovedProvidersQuery Query { get; set; }
        public List<Cohort> Cohorts { get; set; }
        public List<Provider> Provider { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<GetApprovedProvidersQuery, GetApprovedProvidersQueryResult> Handler { get; set; }

        public long AccountId => 1;

        public GetApprovedProvidersQueryHandlerFixture()
        {
            Cohorts = new List<Cohort>();
            Provider = new List<Provider>();
            Query = new GetApprovedProvidersQuery(AccountId);
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetApprovedProvidersQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public GetApprovedProvidersQueryHandlerFixture AddCohortAndProvider_WithTransfserSenderNotApproved()
        {
            var provider = new Provider(GetNextProviderId(), "Foo", DateTime.UtcNow, DateTime.UtcNow);

            Provider.Add(provider);

            Cohorts.Add(new Cohort
            {
                EmployerAccountId = AccountId,
                Id = GetNextCohortId(),
                EditStatus = 0,
                TransferSenderId = 1,
                ProviderId = provider.UkPrn,
                TransferApprovalStatus = Types.TransferApprovalStatus.Pending
            });
            
            return this;
        }

        public GetApprovedProvidersQueryHandlerFixture AddCohortAndProvider_WithTransferSenderApproved()
        {
            var provider = new Provider(GetNextProviderId(), "Foo", DateTime.UtcNow, DateTime.UtcNow);

            Provider.Add(provider);

            Cohorts.Add(new Cohort
            {
                EmployerAccountId = AccountId,
                Id = GetNextCohortId(),
                EditStatus = 0,
                TransferSenderId = 1,
                ProviderId = provider.UkPrn,
                TransferApprovalStatus = Types.TransferApprovalStatus.Approved
            });

            return this;
        }

        public GetApprovedProvidersQueryHandlerFixture AddApprovedCohortAndProviderForAccount()
        {
            var provider = new Provider(GetNextProviderId(), "Foo", DateTime.UtcNow, DateTime.UtcNow);

            Provider.Add(provider);

            Cohorts.Add(new Cohort
            {
                EmployerAccountId = AccountId,
                Id = GetNextCohortId(),
                EditStatus = 0,
                TransferSenderId = null,
                ProviderId = provider.UkPrn
            });

            return this;
        }

        public GetApprovedProvidersQueryHandlerFixture AddNotApprovedCohortAndProviderForAccount()
        {
            Provider.Add(
                new Provider(GetNextProviderId(), "Foo", DateTime.UtcNow, DateTime.UtcNow)
              );

            Cohorts.Add(new Cohort
            {
                EmployerAccountId = AccountId,
                Id = GetNextCohortId(),
                EditStatus = Types.EditStatus.EmployerOnly,
                TransferSenderId = null,
                ProviderId = 1
            });

            return this;
        }

        public Task<GetApprovedProvidersQueryResult> Handle()
        {
            return Handler.Handle(Query, CancellationToken.None);
        }

        public GetApprovedProvidersQueryHandlerFixture SeedDb()
        {
            Db.Cohorts.AddRange(Cohorts);
            Db.Providers.AddRange(Provider);
            Db.SaveChanges();

            return this;
        }

        private long GetNextCohortId() => Cohorts.Count == 0 ? 1 : Cohorts.Max(x => x.Id) + 1;

        private long GetNextProviderId() => Provider.Count == 0 ? 1:  Provider.Max(x => x.UkPrn) + 1;

    }
}
