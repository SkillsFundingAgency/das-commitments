using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentValidation;
using FluentValidation.Internal;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using Message = SFA.DAS.CommitmentsV2.Models.Message;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortSummary
{
    [TestFixture]
    public class GetCohortSummaryQueryHandlerTests
    {
        public Cohort Cohort;
        public AccountLegalEntity AccountLegalEntity;
        public Provider Provider;
        public long CohortId;
        public long AccountLegalEntityId;
        public Party WithParty = Party.Employer;
        public const string LatestMessageCreatedByEmployer = "ohayou";
        public const string LatestMessageCreatedByProvider = "konbanwa";
        public bool HasTransferSender = true;
        public bool CohortIsDeleted;
        public Party Approvals;
        public ApprenticeshipEmployerType LevyStatus = ApprenticeshipEmployerType.NonLevy;
        public long? ChangeOfPartyRequestId;

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
            await CheckQueryResponse(response => Assert.AreEqual(Provider.Name, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityName()
        {
            await CheckQueryResponse(response => Assert.AreEqual(AccountLegalEntity.Name, response.LegalEntityName, "Did not return expected legal entity name"));
        }

        [TestCase(Party.Employer, Party.Employer)]
        [TestCase(Party.Provider, Party.Provider)]
        [TestCase(Party.None, Party.None)]
        [TestCase(Party.TransferSender, Party.TransferSender)]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedParty(Party withParty, Party expectedParty)
        {
            WithParty = withParty;
            HasTransferSender = true;
            await CheckQueryResponse(response => Assert.AreEqual(expectedParty, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithTransferSender_ShouldReturnTransferSender()
        {
            WithParty = Party.TransferSender;
            HasTransferSender = false;
            await CheckQueryResponse(response => Assert.AreEqual(Party.TransferSender, response.WithParty, "Did not return expected Party type"));
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
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.AccountLegalEntity.PublicHashedId, response.AccountLegalEntityPublicHashedId, "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(AccountLegalEntity.Id, response.AccountLegalEntityId, "Did not return expected account legal entity ID"));
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
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedEmployerAccountId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.EmployerAccountId, response.AccountId, "Did not return expected EmployerAccountId"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByProviderEmail()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastUpdatedByProviderEmail, response.LastUpdatedByProviderEmail, "Did not return expected Last Updated By Provider Email"));
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, true)]
        [TestCase(Party.Employer | Party.Provider, true)]
        [TestCase(null, false)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedIsApprovedByEmployer(Party approvals, bool expectIsApprovedByEmployer)
        {
            WithParty = Party.Employer;
            Approvals = approvals;
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
        [TestCase(8, true)]
        [TestCase(9, true)]

        public async Task Handle_WithApprenticeDetails_ShouldReturnExpectedEmployerCanApprove(int nullProperty, bool expectedEmployerCanApprove)
        {
            var apprenticeDetails = SetApprenticeDetails(nullProperty);

            await CheckQueryResponse(response => Assert.AreEqual(expectedEmployerCanApprove, response.IsCompleteForEmployer),
                apprenticeDetails);
        }

        [TestCase(false, true, true, false)]
        [TestCase(false, true, false,true)]
        [TestCase(true, true, true, true)]
        [TestCase(true, true, false, true)]
        [TestCase(false, false, false, true)]
        [TestCase(true, false, false, true)]
        public async Task Handle_WithApprenticeEmail_ShouldReturnExpectedEmployerCanApprove(bool emailPresent, bool apprenticeEmailRequired, bool onPrivateBetaList, bool expectedCanApprove)
        {
            var fieldToSet = emailPresent ? 0 : 8;
            Action<GetCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.ApprenticeEmailFeatureServiceMock.Setup(x => x.IsEnabled).Returns(apprenticeEmailRequired);
                f.ApprenticeEmailFeatureServiceMock
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>())).Returns(onPrivateBetaList);
            });

            var apprenticeDetails = SetApprenticeDetails(fieldToSet);

            await CheckQueryResponse(response =>
                {
                    Assert.AreEqual(expectedCanApprove, response.IsCompleteForEmployer);
                    Assert.AreEqual(expectedCanApprove, response.IsCompleteForProvider);
                },
                apprenticeDetails, arrange);
        }

        [TestCase(false, null, false)]
        [TestCase(true, null, true)]
        [TestCase(true, 101, true)]
        public async Task Handle_WithApprenticeEmailAndAContinuationOfId_ShouldReturnExpectedEmployerCanApprove(bool emailPresent, long? continuationOfId, bool expectedCanApprove)
        {
            var fieldToSet = emailPresent ? 0 : 8;
            Action<GetCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.ApprenticeEmailFeatureServiceMock.Setup(x => x.IsEnabled).Returns(true);
                f.ApprenticeEmailFeatureServiceMock
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            });

            var apprenticeDetails = SetApprenticeDetails(fieldToSet);

            await CheckQueryResponse(response =>
                {
                    Assert.AreEqual(expectedCanApprove, response.IsCompleteForEmployer);
                    Assert.AreEqual(expectedCanApprove, response.IsCompleteForProvider);
                },
                apprenticeDetails, arrange, continuationOfId);
        }


        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        [TestCase(5, false)]
        [TestCase(6, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        public async Task Handle_WithApprenticeDetails_ShouldReturnExpectedProviderCanApprove(int nullProperty, bool expectedProviderCanApprove)
        {
            var apprenticeDetails = SetApprenticeDetails(nullProperty);

            await CheckQueryResponse(response => Assert.AreEqual(expectedProviderCanApprove, response.IsCompleteForProvider),
                apprenticeDetails);
        }

        [Test]
        public async Task Handle_WithNoApprenticeDetails_ShouldReturnEmployerCannotApprove()
        {
            await CheckQueryResponse(response => Assert.IsFalse(response.IsCompleteForEmployer));
        }

        [Test]
        [NonParallelizable]
        public async Task Handle_DeletedCohort_ShouldReturnNull()
        {
            CohortIsDeleted = true;
            await CheckQueryResponse(Assert.IsNull);
        }

        [TestCase(ApprenticeshipEmployerType.Levy)]
        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedLevyStatus(ApprenticeshipEmployerType levyStatus)
        {
            LevyStatus = levyStatus;
            await CheckQueryResponse(response => Assert.AreEqual(LevyStatus, response.LevyStatus, "Did not return expected LevyStatus"));
		}

        [TestCase(null)]
        [TestCase(123)]
        public async Task Handle_CohortWithChangeOfParty_ShouldReturnChangeOfPartyRequestId(long? value)
        {
            ChangeOfPartyRequestId = value;
            await CheckQueryResponse(response => Assert.AreEqual(value, response.ChangeOfPartyRequestId));
        }
		
        private async Task CheckQueryResponse(Action<GetCohortSummaryQueryResult> assert, DraftApprenticeshipDetails apprenticeshipDetails = null, Action<GetCohortSummaryHandlerTestFixtures> arrange = null, long? continuationOfId = null)
        {
            var autoFixture = new Fixture();

            var account = new Account(autoFixture.Create<long>(), "", "", "", DateTime.UtcNow) { LevyStatus = LevyStatus };
            AccountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", autoFixture.Create<string>(),
                OrganisationType.Charities, "", DateTime.UtcNow);
            Provider = new Provider{Name =autoFixture.Create<string>()};
            
            CohortId = autoFixture.Create<long>();
            Cohort = autoFixture.Build<Cohort>().Without(o=>o.Apprenticeships).Without(o=>o.TransferRequests).Without(o=>o.Messages)
                .Without(o=>o.AccountLegalEntity).Without(o=>o.Provider).Without(o => o.TransferSender)
                .With(o=>o.WithParty, Party.Provider)
                .Create();
            Cohort.AccountLegalEntity = AccountLegalEntity;
            Cohort.Provider = Provider;

            Cohort.IsDeleted = CohortIsDeleted;
            if (!HasTransferSender)
            {
                Cohort.TransferSenderId = null;
            }
            
            if (apprenticeshipDetails != null)
            {
                var draftApprenticeship = new DraftApprenticeship(apprenticeshipDetails, Cohort.WithParty);
                draftApprenticeship.ContinuationOfId = continuationOfId;
                Cohort.Apprenticeships.Add(draftApprenticeship);
            }

            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, Cohort, WithParty, LatestMessageCreatedByEmployer, LatestMessageCreatedByProvider, Approvals, ChangeOfPartyRequestId);

            arrange?.Invoke(fixtures);

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
                Email = "test@test.com",
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("code", "name", ProgrammeType.Framework, DateTime.Now, DateTime.Now),
                Cost = 1500,
                StartDate = new DateTime(2019, 10, 1),
                EndDate = DateTime.Now,
                DateOfBirth = new DateTime(2000, 1, 1),
                Reference = "",
                ReservationId = new Guid(),
                Uln = "1234567890"
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
                case 8:
                    apprenticeDetails.Email = null;
                    break;
                case 9:
                    apprenticeDetails.Uln = null;
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
            ApprenticeEmailFeatureServiceMock = new Mock<IApprenticeEmailFeatureService>();
            SeedCohorts = new List<Cohort>();
        }

        public Mock<IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>> HandlerMock { get; set; }

        public IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetCohortSummaryQuery>> ValidatorMock { get; set; }
        public Mock<IApprenticeEmailFeatureService> ApprenticeEmailFeatureServiceMock { get; set; }
        public IValidator<GetCohortSummaryQuery> Validator => ValidatorMock.Object;

        public List<Cohort> SeedCohorts { get; }

        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, Cohort cohort, Party withParty, string latestMessageCreatedByEmployer, string latestMessageCreatedByProvider, Party approvals, long? changeOfPartyRequestId)
        {
            cohort.Id =  cohortId;
            cohort.WithParty = withParty;

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

            cohort.Approvals = approvals;
            cohort.ChangeOfPartyRequestId = changeOfPartyRequestId;

            SeedCohorts.Add(cohort);

            return this;
        }

        public Task<GetCohortSummaryQueryResult> GetResult(GetCohortSummaryQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortSummaryQueryHandler(lazy, ApprenticeEmailFeatureServiceMock.Object);

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
