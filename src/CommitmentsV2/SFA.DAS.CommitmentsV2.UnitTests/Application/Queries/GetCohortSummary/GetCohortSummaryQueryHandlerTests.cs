using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
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
        public Cohort Cohort;
        public long CohortId;
        public long AccountLegalEntityId;
        public EditStatus EditStatus;
        public const string LatestMessageCreatedByEmployer = "ohayou";
        public const string LatestMessageCreatedByProvider = "konbanwa";

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
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortReference()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.Reference, response.CohortReference, "Did not return expected cohort reference"));
        }


        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.ProviderName, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityNameName()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.LegalEntityName, response.LegalEntityName, "Did not return expected legal entity name"));
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
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.AccountLegalEntityPublicHashedId, response.AccountLegalEntityPublicHashedId, "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            return CheckCommandResponse(response => Assert.AreEqual(AccountLegalEntityId, response.AccountLegalEntityId, "Did not return expected account legal entity ID"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLastAction()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.LastAction, response.LastAction, "Did not return expected Last Action"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByEmployerEmail()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.LastUpdatedByEmployerEmail, response.LastUpdatedByEmployerEmail, "Did not return expected Last Updated By Employer Email"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByProviderEmail()
        {
            return CheckCommandResponse(response => Assert.AreEqual(Cohort.LastUpdatedByProviderEmail, response.LastUpdatedByProviderEmail, "Did not return expected Last Updated By Provider Email"));
        }

        private async Task CheckCommandResponse(Action<GetCohortSummaryQueryResult> assert)
        {
            var autoFixture = new Fixture();

            CohortId = autoFixture.Create<long>();
            Cohort = autoFixture.Build<Cohort>().Without(o=>o.Apprenticeships).Without(o=>o.TransferRequests).Without(o=>o.Messages).Create();
            AccountLegalEntityId = autoFixture.Create<long>();

            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, Cohort, EditStatus, AccountLegalEntityId, LatestMessageCreatedByEmployer, LatestMessageCreatedByProvider);

            // act
            var response = await fixtures.GetResult(new GetCohortSummaryQuery { CohortId = CohortId });

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

        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, Cohort cohort, EditStatus editStatus, long decodedAccountLegalEntityId, string latestMessageCreatedByEmployer, string latestMessageCreatedByProvider)
        {
            cohort.Id =  cohortId;
            cohort.EditStatus = editStatus;

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

            SeedCohorts.Add(cohort);
            
            EncodingServiceMock
                .Setup(e => e.Decode(cohort.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(decodedAccountLegalEntityId);

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