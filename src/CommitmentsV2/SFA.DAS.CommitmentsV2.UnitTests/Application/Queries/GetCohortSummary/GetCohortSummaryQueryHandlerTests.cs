using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortSummary
{
    [TestFixture]
    public class GetCohortSummaryQueryHandlerTests
    {
        const long CohortId = 456;
        const string AccountLegalEntityPublicHashedId = "ALE789";
        const long AccountLegalEntityId = 789;
        const string LegalEntityName = "ACME Fireworks";
        const string ProviderName = "ACME Training";
        public EditStatus EditStatus = EditStatus.EmployerOnly;
        public string LatestMessageCreatedByEmployer = "ohayou";
        public string LatestMessageCreatedByProvider = "konbanwa";
        public AgreementStatus ApprenticeshipAgreementStatus = AgreementStatus.NotAgreed;

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnValue()
        {
            return CheckCommandResponse(response => Assert.IsNotNull(response, "Did not return response"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortId()
        {
            return CheckCommandResponse(response => Assert.AreEqual(CohortId, response.CohortId, "Did not return expected cohort id"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            return CheckCommandResponse(response => Assert.AreEqual(ProviderName, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityNameName()
        {
            return CheckCommandResponse(response => Assert.AreEqual(LegalEntityName, response.LegalEntityName, "Did not return expected legal entity name"));
        }

        [TestCase(EditStatus.EmployerOnly, Party.Employer)]
        [TestCase(EditStatus.ProviderOnly, Party.Provider)]
        [TestCase(EditStatus.Neither, Party.None)]
        [TestCase(EditStatus.Both, Party.None)]
        public Task Handle_WithSpecifiedIdAndEditStatus_ShouldReturnExpectedParty(EditStatus editStatus, Party expectedParty)
        {
            EditStatus = editStatus;
            return CheckCommandResponse(response => Assert.AreEqual(expectedParty, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByEmployer()
        {
            return CheckCommandResponse(response => Assert.AreEqual(LatestMessageCreatedByEmployer, response.LatestMessageCreatedByEmployer, "Did not return expected latest message created by employer"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByProvider()
        {
            return CheckCommandResponse(response => Assert.AreEqual(LatestMessageCreatedByProvider, response.LatestMessageCreatedByProvider, "Did not return expected latest message created by provider"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityPublicHashedId()
        {
            return CheckCommandResponse(response => Assert.AreEqual(AccountLegalEntityPublicHashedId, response.AccountLegalEntityPublicHashedId, "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            return CheckCommandResponse(response => Assert.AreEqual(AccountLegalEntityId, response.AccountLegalEntityId, "Did not return expected account legal entity ID"));
        }

        [TestCase(AgreementStatus.NotAgreed, false)]
        [TestCase(AgreementStatus.ProviderAgreed, false)]
        [TestCase(AgreementStatus.EmployerAgreed, true)]
        [TestCase(AgreementStatus.BothAgreed, true)]
        public Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedIsApprovedByEmployer(AgreementStatus agreementStatus, bool expectIsApprovedByEmployer)
        {
            ApprenticeshipAgreementStatus = agreementStatus;
             return CheckCommandResponse(response => Assert.AreEqual(expectIsApprovedByEmployer, response.IsApprovedByEmployer, "Did not return expected IsApprovedByEmployer"));
        }

        private async Task CheckCommandResponse(Action<GetCohortSummaryQueryResult> assert)
        {
            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, AccountLegalEntityPublicHashedId, AccountLegalEntityId, LegalEntityName, ProviderName, EditStatus, LatestMessageCreatedByEmployer, LatestMessageCreatedByProvider, ApprenticeshipAgreementStatus);

            // act
            var response = await fixtures.GetResult(new GetCohortSummaryQuery(CohortId));

            // Assert
            assert(response);
        }
    }

    public class GetCohortSummaryHandlerTestFixtures
    {
        public GetCohortSummaryHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>>();    
            ValidatorMock = new Mock<IValidator<GetCohortSummaryQuery>>();
            SeedCohorts = new List<Cohort>();
            EncodingServiceMock = new Mock<IEncodingService>();
        }

        public Mock<IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>> HandlerMock { get; set; }

        public IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetCohortSummaryQuery>> ValidatorMock { get; set; }
        public IValidator<GetCohortSummaryQuery> Validator => ValidatorMock.Object;

        public List<Cohort> SeedCohorts { get; }
        public Mock<IEncodingService> EncodingServiceMock { get; set; }
        
        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, string accountLegalEntityPublicHashedId, long accountLegalEntityId, string legalEntityName, string providerName, EditStatus editStatus, string latestMessageCreatedByEmployer, string latestMessageCreatedByProvider, AgreementStatus apprenticeshipAgreementStatus)
        {
            var cohort = new Cohort
            {
                LegalEntityId = legalEntityName,
                LegalEntityName = legalEntityName,
                LegalEntityAddress = "An Address",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = editStatus,
                LastAction = LastAction.None,
                Originator = Originator.Unknown,
                ProviderName = providerName,
                Id = cohortId,
                Reference = string.Empty,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };

            cohort.Messages.Add(new Message
            {
                CreatedBy = 0,
                CreatedDateTime = DateTime.UtcNow.AddDays(-1),
                Text = "Foo"
            });
            
            cohort.Messages.Add(new Message
            {
                CreatedBy = 0,
                CreatedDateTime = DateTime.UtcNow,
                Text = latestMessageCreatedByEmployer
            });
            
            cohort.Messages.Add(new Message
            {
                CreatedBy = 1,
                CreatedDateTime = DateTime.UtcNow.AddDays(-1),
                Text = "Bar"
            });
            
            cohort.Messages.Add(new Message
            {
                CreatedBy = 1,
                CreatedDateTime = DateTime.UtcNow,
                Text = latestMessageCreatedByProvider
            });

            cohort.Apprenticeships.Add(new DraftApprenticeship
            {
                AgreementStatus = apprenticeshipAgreementStatus
            });

            SeedCohorts.Add(cohort);
            
            EncodingServiceMock
                .Setup(e => e.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            return this;
        }

        public Task<GetCohortSummaryQueryResult> GetResult(GetCohortSummaryQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortSummaryQueryHandler(lazy, EncodingServiceMock.Object);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(databaseName: "SFA.DAS.Commitments.Database")
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            using (var dbContext = new ProviderCommitmentsDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            }
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Cohorts.AddRange(SeedCohorts);
            dbContext.SaveChanges(true);
        }

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[]
            {
#pragma warning disable 618
                new ConsoleLoggerProvider((category, level)
#pragma warning restore 618
                    => category == DbLoggerCategory.Database.Command.Name
                       && level == LogLevel.Debug, true)
            });
    }
}