using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetEmployerCohortsReadyForApproval
{
    [TestFixture]
    [Parallelizable]
    public class GetEmployerCohortsReadyForApprovalQueryHandlerTests
    {
        private GetEmployerCohortsReadyForApprovalQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetEmployerCohortsReadyForApprovalQueryHandlerTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_Should_Return_Ready_For_Approval()
        {
            var result = await _fixture.Handle();

            Assert.IsNotNull(result);

            Assert.IsInstanceOf<GetEmployerCohortsReadyForApprovalQueryResults>(result);
            Assert.IsNotNull(result.GetEmployerCohortsReadyForApprovalQueryResult);
            Assert.IsTrue(result.GetEmployerCohortsReadyForApprovalQueryResult.Any(c => c.CohortId == 1));
        }

        [Test]
        public async Task Handle_Should_Return_Draft_Ready_For_Approval()
        {
            var result = await _fixture.Handle();

            Assert.IsNotNull(result);

            Assert.IsInstanceOf<GetEmployerCohortsReadyForApprovalQueryResults>(result);
            Assert.IsNotNull(result.GetEmployerCohortsReadyForApprovalQueryResult);
            Assert.IsTrue(result.GetEmployerCohortsReadyForApprovalQueryResult.Any(c => c.CohortId == 2));

        }

        [Test]
        public async Task Handle_Should_Not_Return_Invalid_Cohort()
        {
            var result = await _fixture.Handle();

            Assert.IsNotNull(result);

            Assert.IsInstanceOf<GetEmployerCohortsReadyForApprovalQueryResults>(result);
            Assert.IsNotNull(result.GetEmployerCohortsReadyForApprovalQueryResult);
            Assert.IsFalse(result.GetEmployerCohortsReadyForApprovalQueryResult.Any(c => c.CohortId == 3));
        }
    }

    public sealed class GetEmployerCohortsReadyForApprovalQueryHandlerTestsFixture : IDisposable
    {
        public readonly int _employerAccountID = 123;
        public GetEmployerCohortsReadyForApprovalQuery Query { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<GetEmployerCohortsReadyForApprovalQuery, GetEmployerCohortsReadyForApprovalQueryResults> Handler { get; set; }

        public GetEmployerCohortsReadyForApprovalQueryHandlerTestsFixture()
        {
            Query = new GetEmployerCohortsReadyForApprovalQuery(_employerAccountID);
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetEmployerCohortsReadyForApprovalQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));

            var _ = SeedData().Result;
        }

        public async Task<GetEmployerCohortsReadyForApprovalQueryHandlerTestsFixture> SeedData()
        {
            var accountLegalEntity1 = new AccountLegalEntity()
                  .Set(x => x.Id, 1);

            var account1 = new Account().Set(c => c.Id, 1);

            var cohortReadyForApproval = new Cohort()
                .Set(c => c.Id, 1)
                .Set(c => c.EmployerAccountId, _employerAccountID)
                .Set(c => c.IsDeleted, false)
                .Set(c => c.Approvals, Party.Provider)
                .Set(c => c.WithParty, Party.Employer)
                .Set(c => c.Provider, new Provider())
                .Set(c => c.TransferSender, account1)
                .Set(c => c.AccountLegalEntity, accountLegalEntity1);

            var account2 = new Account().Set(c => c.Id, 2);

            var cohortDraftReadyForApproval = new Cohort()
                .Set(c => c.Id, 2)
                .Set(c => c.EmployerAccountId, _employerAccountID)
                .Set(c => c.IsDeleted, false)
                .Set(c => c.Approvals, Party.None)
                .Set(c => c.WithParty, Party.Employer)
                .Set(c => c.Originator, Party.Employer.ToOriginator())
                .Set(c => c.IsDraft, true)
                .Set(c => c.Provider, new Provider())
                .Set(c => c.TransferSender, account2)
                .Set(c => c.AccountLegalEntity, accountLegalEntity1);

            var account3 = new Account().Set(c => c.Id, 3);

            var cohortNotReadyForApproval = new Cohort()
                .Set(c => c.Id, 3)
                .Set(c => c.EmployerAccountId, _employerAccountID)
                .Set(c => c.IsDeleted, false)
                .Set(c => c.Approvals, Party.Employer)
                .Set(c => c.WithParty, Party.Provider)
                .Set(c => c.Provider, new Provider())
                .Set(c => c.TransferSender, account3)
                .Set(c => c.AccountLegalEntity, accountLegalEntity1);

            Db.Cohorts.Add(cohortReadyForApproval);
            Db.Cohorts.Add(cohortDraftReadyForApproval);
            Db.Cohorts.Add(cohortNotReadyForApproval);

            await Db.SaveChangesAsync();
            return this;
        }

        public Task<GetEmployerCohortsReadyForApprovalQueryResults> Handle()
        {
            return Handler.Handle(Query, CancellationToken.None);
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}