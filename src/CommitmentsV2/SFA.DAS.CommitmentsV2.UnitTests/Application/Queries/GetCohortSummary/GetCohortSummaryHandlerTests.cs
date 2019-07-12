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

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortSummary
{
    [TestFixture]
    public class GetCohortSummaryHandlerTests
    {
        const long CohortId = 456;
        const string LegalEntityId = "333455";
        const string LegalEntityName = "ACME Fireworks";
        const string ProviderName = "ACME Training";
        public EditStatus EditStatus = EditStatus.Both; 

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
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityId()
        {
            return CheckCommandResponse(response => Assert.AreEqual(LegalEntityId, response.LegalEntityId, "Did not return expected Legal Entity id"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            return CheckCommandResponse(response => Assert.AreEqual(ProviderName, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityName()
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

        private async Task CheckCommandResponse(Action<GetCohortSummaryResponse> assert)
        {
            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, LegalEntityId, LegalEntityName, ProviderName, EditStatus);

            // act
            var response = await fixtures.GetResponse(new GetCohortSummaryRequest { CohortId = CohortId });

            // Assert
            assert(response);
        }
    }

    public class GetCohortSummaryHandlerTestFixtures
    {
        public GetCohortSummaryHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetCohortSummaryRequest, GetCohortSummaryResponse>>();    
            ValidatorMock = new Mock<IValidator<GetCohortSummaryRequest>>();
            SeedCohorts = new List<Cohort>();
        }

        public Mock<IRequestHandler<GetCohortSummaryRequest, GetCohortSummaryResponse>> HandlerMock { get; set; }

        public IRequestHandler<GetCohortSummaryRequest, GetCohortSummaryResponse> Handler => HandlerMock.Object;

        public Mock<IValidator<GetCohortSummaryRequest>> ValidatorMock { get; set; }
        public IValidator<GetCohortSummaryRequest> Validator => ValidatorMock.Object;

        public List<Cohort> SeedCohorts { get; }

        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, string legalEntityId, string legalEntityName, string providerName, EditStatus editStatus)
        {
            var cohort = new Cohort
            {
                LegalEntityId = legalEntityId,
                LegalEntityName = legalEntityName,
                LegalEntityAddress = "An Address",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = editStatus,
                LastAction = LastAction.None,
                Originator = Originator.Unknown,
                ProviderName = providerName,
                Id = cohortId,
                Reference = string.Empty
            };

            SeedCohorts.Add(cohort);

            return this;
        }

        public Task<GetCohortSummaryResponse> GetResponse(GetCohortSummaryRequest request)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortSummaryHandler(lazy);

                return handler.Handle(request, CancellationToken.None);
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