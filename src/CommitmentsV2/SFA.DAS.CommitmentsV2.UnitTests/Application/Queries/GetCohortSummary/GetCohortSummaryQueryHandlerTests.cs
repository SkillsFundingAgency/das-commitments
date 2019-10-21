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
using SFA.DAS.CommitmentsV2.Domain.Entities;
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
        public EditStatus EditStatus = EditStatus.EmployerOnly;
        public const string LatestMessageCreatedByEmployer = "ohayou";
        public const string LatestMessageCreatedByProvider = "konbanwa";
        public bool HasTransferSender = true;
        public AgreementStatus? ApprenticeshipAgreementStatus = AgreementStatus.NotAgreed;

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnValue()
        {
            await CheckQueryResponse(response => Assert.IsNotNull(response, "Did not return response"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(CohortId, response.CohortId, "Did not return expected cohort id"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortReference()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.Reference, response.CohortReference, "Did not return expected cohort reference"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.ProviderName, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityNameName()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LegalEntityName, response.LegalEntityName, "Did not return expected legal entity name"));
        }

        [TestCase(EditStatus.EmployerOnly, Party.Employer)]
        [TestCase(EditStatus.ProviderOnly, Party.Provider)]
        [TestCase(EditStatus.Neither, Party.None)]
        [TestCase(EditStatus.Both, Party.TransferSender)]
        public async Task Handle_WithSpecifiedIdAndEditStatus_ShouldReturnExpectedParty(EditStatus editStatus, Party expectedParty)
        {
            EditStatus = editStatus;
            HasTransferSender = true;
            await CheckQueryResponse(response => Assert.AreEqual(expectedParty, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithEditStatusOfBothAndHasTransferSender_ShouldReturnTransferSender()
        {
            EditStatus = EditStatus.Both;
            HasTransferSender = false;
            await CheckQueryResponse(response => Assert.AreEqual(Party.None, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByEmployer()
        {
            await CheckQueryResponse(response => Assert.AreEqual(LatestMessageCreatedByEmployer, response.LatestMessageCreatedByEmployer, "Did not return expected latest message created by employer"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByProvider()
        {
            await CheckQueryResponse(response => Assert.AreEqual(LatestMessageCreatedByProvider, response.LatestMessageCreatedByProvider, "Did not return expected latest message created by provider"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityPublicHashedId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.AccountLegalEntityPublicHashedId, response.AccountLegalEntityPublicHashedId, "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(AccountLegalEntityId, response.AccountLegalEntityId, "Did not return expected account legal entity ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastAction()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastAction, response.LastAction, "Did not return expected Last Action"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByEmployerEmail()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastUpdatedByEmployerEmail, response.LastUpdatedByEmployerEmail, "Did not return expected Last Updated By Employer Email"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByProviderEmail()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastUpdatedByProviderEmail, response.LastUpdatedByProviderEmail, "Did not return expected Last Updated By Provider Email"));
        }

        [TestCase(AgreementStatus.NotAgreed, false)]
        [TestCase(AgreementStatus.ProviderAgreed, false)]
        [TestCase(AgreementStatus.EmployerAgreed, true)]
        [TestCase(AgreementStatus.BothAgreed, true)]
        [TestCase(null, false)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedIsApprovedByEmployer(AgreementStatus? agreementStatus, bool expectIsApprovedByEmployer)
        {
            EditStatus = EditStatus.EmployerOnly;
            ApprenticeshipAgreementStatus = agreementStatus;
            await CheckQueryResponse(response => Assert.AreEqual(expectIsApprovedByEmployer, response.IsApprovedByEmployer, "Did not return expected IsApprovedByEmployer"));
        }

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        [TestCase(5, false)]
        [TestCase(6, false)]
        [TestCase(7, false)]
        public async Task Handle_WithApprenticeDetails_ShouldReturnExpectedEmployerCanApprove(int nullProperty, bool expectedEmployerCanApprove)
        {
            var apprenticeDetails = SetApprenticeDetails(nullProperty);

            await CheckQueryResponse(response => Assert.AreEqual(expectedEmployerCanApprove, response.IsCompleteForEmployer),
                apprenticeDetails);
        }

        private async Task CheckQueryResponse(Action<GetCohortSummaryQueryResult> assert, DraftApprenticeshipDetails apprenticeshipDetails = null)
        {
            var autoFixture = new Fixture();

            CohortId = autoFixture.Create<long>();
            Cohort = autoFixture.Build<Cohort>().Without(o=>o.Apprenticeships).Without(o=>o.TransferRequests).Without(o=>o.Messages).Create();
            
            if (!HasTransferSender)
            {
                Cohort.TransferSenderId = null;
            }
            AccountLegalEntityId = autoFixture.Create<long>();
            if (apprenticeshipDetails != null)
            {
                Cohort.Apprenticeships.Add(new DraftApprenticeship(apprenticeshipDetails, Cohort.WithParty));
                ApprenticeshipAgreementStatus = null;
            }

            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, Cohort, EditStatus, AccountLegalEntityId, LatestMessageCreatedByEmployer, LatestMessageCreatedByProvider, ApprenticeshipAgreementStatus);

            // act
            var response = await fixtures.GetResult(new GetCohortSummaryQuery(CohortId));

            // Assert
            assert(response);
        }


        private DraftApprenticeshipDetails SetApprenticeDetails(int nullProperty)
        {
            var apprenticeDetails = new DraftApprenticeshipDetails
            {
                Id = 1,
                FirstName = "FirstName",
                LastName = "LastName",
                TrainingProgramme = new TrainingProgramme("code", "name", ProgrammeType.Framework, DateTime.Now, DateTime.Now),
                Cost = 1500,
                StartDate = new DateTime(2019, 10, 1),
                EndDate = DateTime.Now,
                DateOfBirth = new DateTime(2000, 1, 1),
                Reference = "",
                ReservationId = new Guid()
            };
            switch (nullProperty)
            {
                case 1:
                    apprenticeDetails.FirstName = null;
                    break;
                case 2:
                    apprenticeDetails.LastName = null;
                    break;
                case 3:
                    apprenticeDetails.TrainingProgramme = null;
                    break;
                case 4:
                    apprenticeDetails.Cost = null;
                    break;
                case 5:
                    apprenticeDetails.StartDate = null;
                    break;
                case 6:
                    apprenticeDetails.EndDate = null;
                    break;
                case 7:
                    apprenticeDetails.DateOfBirth = null;
                    break;
            }
            return apprenticeDetails;
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

        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, Cohort cohort, EditStatus editStatus, long decodedAccountLegalEntityId, string latestMessageCreatedByEmployer, string latestMessageCreatedByProvider, AgreementStatus? apprenticeshipAgreementStatus)
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

            if (apprenticeshipAgreementStatus.HasValue)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship
                {
                    AgreementStatus = apprenticeshipAgreementStatus.Value
                });
            }

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