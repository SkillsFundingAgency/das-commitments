using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountTransferStatus
{
    [TestFixture]
    public class GetAccountTransferStatusQueryHandlerTests
    {
        private GetAccountTransferStatusQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new GetAccountTransferStatusQueryHandlerTestsFixture();
        }

        [Test]
        public async Task When_Getting_Transfer_Status_Then_IsSender_Is_True_When_Employer_Is_Transfer_Sender()
        {
            _fixture.WithEmployerAsTransferSender();
            await _fixture.Handle();
            _fixture.VerifyIsTransferSender(true);
        }

        [Test]
        public async Task When_Getting_Transfer_Status_Then_IsSender_Is_False_When_Employer_Is_Not_Transfer_Sender()
        {
            _fixture.WithNoActiveApprenticeshipsForEmployer();
            await _fixture.Handle();
            _fixture.VerifyIsTransferSender(false);
        }

        [Test]
        public async Task When_Getting_Transfer_Status_Then_IsReceiver_Is_True_When_Employer_Is_Transfer_Receiver()
        {
            _fixture.WithEmployerAsTransferReceiver();
            await _fixture.Handle();
            _fixture.VerifyIsTransferReceiver(true);
        }

        [Test]
        public async Task When_Getting_Transfer_Status_Then_IsReceiver_Is_False_When_Employer_Is_Not_Transfer_Receiver()
        {
            _fixture.WithNoActiveApprenticeshipsForEmployer();
            await _fixture.Handle();
            _fixture.VerifyIsTransferReceiver(false);
        }


        public class GetAccountTransferStatusQueryHandlerTestsFixture
        {
            public ProviderCommitmentsDbContext Db { get; set; }
            public Mock<IAuthenticationService> AuthenticationServiceMock { get; set; }
            public GetAccountTransferStatusQueryHandler Handler { get; set; }
            private readonly Fixture _autoFixture = new Fixture();

            public GetAccountTransferStatusQuery Query { get; set; }
            private GetAccountTransferStatusQueryResult Result { get; set; }

            public Provider Provider { get; set; }
            public Account Account { get; set; }
            public AccountLegalEntity AccountLegalEntity { get; set; }

            public GetAccountTransferStatusQueryHandlerTestsFixture()
            {
                AuthenticationServiceMock = new Mock<IAuthenticationService>();
                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                Handler = new GetAccountTransferStatusQueryHandler(
                    new Lazy<ProviderCommitmentsDbContext>(() => Db));

                Query = _autoFixture.Create<GetAccountTransferStatusQuery>();

                Provider = new Provider
                {
                    UkPrn = _autoFixture.Create<long>(),
                    Name = _autoFixture.Create<string>()
                };

                Account = new Account(0, "", "", "", DateTime.UtcNow);

                AccountLegalEntity = new AccountLegalEntity(Account,
                    1,
                    0,
                    "",
                    publicHashedId: _autoFixture.Create<string>(),
                    _autoFixture.Create<string>(),
                    OrganisationType.PublicBodies,
                    "",
                    DateTime.UtcNow);

            }

            public async Task Handle()
            {
                Result = await Handler.Handle(Query, CancellationToken.None);
            }

            public void VerifyIsTransferSender(bool isSender)
            {
                Assert.AreEqual(isSender, Result.IsTransferSender);
            }

            public void VerifyIsTransferReceiver(bool isReceiver)
            {
                Assert.AreEqual(isReceiver, Result.IsTransferReceiver);
            }

            public GetAccountTransferStatusQueryHandlerTestsFixture WithNoActiveApprenticeshipsForEmployer()
            {
                // This line is required.
                // ReSharper disable once ObjectCreationAsStatement
                new UnitOfWorkContext();

                var apprenticeships = new List<Apprenticeship>();

                var cohort = new Cohort
                {
                    Id = _autoFixture.Create<long>(),
                    AccountLegalEntity = AccountLegalEntity,
                    EmployerAccountId = Query.AccountId,
                    ProviderId = Provider.UkPrn,
                    Provider = Provider,
                    ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy,
                    TransferSenderId = Query.AccountId
                };

                apprenticeships.Add(new Apprenticeship
                {
                    Id = _autoFixture.Create<int>(),
                    CommitmentId = cohort.Id,
                    Cohort = cohort,
                    PaymentStatus = PaymentStatus.Withdrawn
                });

                apprenticeships.Add(new Apprenticeship
                {
                    Id = _autoFixture.Create<int>(),
                    CommitmentId = cohort.Id,
                    Cohort = cohort,
                    PaymentStatus = PaymentStatus.Completed
                });

                Db.Apprenticeships.AddRange(apprenticeships);
                Db.SaveChanges();

                return this;
            }

            public GetAccountTransferStatusQueryHandlerTestsFixture WithEmployerAsTransferSender()
            {
                // This line is required.
                // ReSharper disable once ObjectCreationAsStatement
                new UnitOfWorkContext();

                var apprenticeships = new List<Apprenticeship>();

                var cohort = new Cohort
                {
                    Id = _autoFixture.Create<long>(),
                    AccountLegalEntity = AccountLegalEntity,
                    EmployerAccountId = _autoFixture.Create<long>(),
                    ProviderId = Provider.UkPrn,
                    Provider = Provider,
                    ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy,
                    TransferSenderId = Query.AccountId
                };

                var apprenticeship = new Apprenticeship
                {
                    Id = _autoFixture.Create<int>(),
                    CommitmentId = cohort.Id,
                    Cohort = cohort,
                    PaymentStatus = PaymentStatus.Active
                };

                apprenticeships.Add(apprenticeship);

                Db.Apprenticeships.AddRange(apprenticeships);
                Db.SaveChanges();

                return this;
            }

            public GetAccountTransferStatusQueryHandlerTestsFixture WithEmployerAsTransferReceiver()
            {
                // This line is required.
                // ReSharper disable once ObjectCreationAsStatement
                new UnitOfWorkContext();

                var apprenticeships = new List<Apprenticeship>();

                var cohort = new Cohort
                {
                    Id = _autoFixture.Create<long>(),
                    AccountLegalEntity = AccountLegalEntity,
                    EmployerAccountId = Query.AccountId,
                    ProviderId = Provider.UkPrn,
                    Provider = Provider,
                    ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy,
                    TransferSenderId = _autoFixture.Create<long>()
                };

                var apprenticeship = new Apprenticeship
                {
                    Id = _autoFixture.Create<int>(),
                    CommitmentId = cohort.Id,
                    Cohort = cohort,
                    PaymentStatus = PaymentStatus.Active
                };

                apprenticeships.Add(apprenticeship);

                Db.Apprenticeships.AddRange(apprenticeships);
                Db.SaveChanges();

                return this;
            }
        }
    }
}
