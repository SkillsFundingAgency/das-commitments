using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Dsl;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using Message = SFA.DAS.CommitmentsV2.Models.Message;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetSupportCohortSummary
{
    [TestFixture]
    public class GetSupportCohortSummaryHandlerTests
    {
        public Cohort Cohort;
        public AccountLegalEntity AccountLegalEntity;
        public Provider Provider;
        public long CohortId;
        public long AccountId;
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
            await CheckQueryResponse(response =>
                Assert.AreEqual(CohortId, response.CohortId, "Did not return expected cohort id"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortReference()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.Reference, response.CohortReference,
                "Did not return expected cohort reference"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            await CheckQueryResponse(response =>
                Assert.AreEqual(Provider.Name, response.ProviderName, "Did not return expected provider name"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityName()
        {
            await CheckQueryResponse(response => Assert.AreEqual(AccountLegalEntity.Name, response.LegalEntityName,
                "Did not return expected legal entity name"));
        }

        [TestCase(Party.Employer, Party.Employer)]
        [TestCase(Party.Provider, Party.Provider)]
        [TestCase(Party.None, Party.None)]
        [TestCase(Party.TransferSender, Party.TransferSender)]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedParty(Party withParty, Party expectedParty)
        {
            WithParty = withParty;
            HasTransferSender = true;
            await CheckQueryResponse(response =>
                Assert.AreEqual(expectedParty, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithTransferSender_ShouldReturnTransferSender()
        {
            WithParty = Party.TransferSender;
            HasTransferSender = false;
            await CheckQueryResponse(response =>
                Assert.AreEqual(Party.TransferSender, response.WithParty, "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByEmployer()
        {
            await CheckQueryResponse(response => Assert.AreEqual(LatestMessageCreatedByEmployer,
                response.LatestMessageCreatedByEmployer, "Did not return expected latest message created by employer"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByProvider()
        {
            await CheckQueryResponse(response => Assert.AreEqual(LatestMessageCreatedByProvider,
                response.LatestMessageCreatedByProvider, "Did not return expected latest message created by provider"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityPublicHashedId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.AccountLegalEntity.PublicHashedId,
                response.AccountLegalEntityPublicHashedId,
                "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(AccountLegalEntity.Id, response.AccountLegalEntityId,
                "Did not return expected account legal entity ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastAction()
        {
            await CheckQueryResponse(response =>
                Assert.AreEqual(Cohort.LastAction, response.LastAction, "Did not return expected Last Action"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByEmployerEmail()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastUpdatedByEmployerEmail,
                response.LastUpdatedByEmployerEmail, "Did not return expected Last Updated By Employer Email"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedEmployerAccountId()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.EmployerAccountId, response.AccountId,
                "Did not return expected EmployerAccountId"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByProviderEmail()
        {
            await CheckQueryResponse(response => Assert.AreEqual(Cohort.LastUpdatedByProviderEmail,
                response.LastUpdatedByProviderEmail, "Did not return expected Last Updated By Provider Email"));
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, true)]
        [TestCase(Party.Employer | Party.Provider, true)]
        [TestCase(null, false)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedIsApprovedByEmployer(Party approvals,
            bool expectIsApprovedByEmployer)
        {
            WithParty = Party.Employer;
            Approvals = approvals;
            await CheckQueryResponse(response => Assert.AreEqual(expectIsApprovedByEmployer,
                response.IsApprovedByEmployer, "Did not return expected IsApprovedByEmployer"));
        }

        private static IEnumerable<TestCaseData> MissingPropertiesTestData
        {
            get
            {
                return DraftsWithMissingProperties().Select(x => new TestCaseData(x.draft.Create(), x.allowedApproval));

                static IEnumerable<(AllowedApproval allowedApproval, IPostprocessComposer<DraftApprenticeshipDetails> draft)> DraftsWithMissingProperties()
                {
                    var completeApprenticeship = new Fixture()
                        .Build<DraftApprenticeshipDetails>()
                        .With(x => x.Email, "person@example.com");

                    yield return (AllowedApproval.BothCanApprove, completeApprenticeship);
                    yield return (AllowedApproval.BothCanApprove, completeApprenticeship.With(x => x.Email, (string)null));

                    yield return (AllowedApproval.EmployerCanApprove, completeApprenticeship.Without(x => x.Uln));

                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.FirstName));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.LastName));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.TrainingProgramme));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.Cost));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.StartDate));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.EndDate));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.DateOfBirth));

                    var flexijobApprenticeship = completeApprenticeship.With(x => x.DeliveryModel, DeliveryModel.PortableFlexiJob);
                    yield return (AllowedApproval.CannotApprove, flexijobApprenticeship.Without(x => x.EmploymentPrice));
                    yield return (AllowedApproval.CannotApprove, flexijobApprenticeship.Without(x => x.EmploymentEndDate));
                }
            }
        }

        [Flags]
        public enum AllowedApproval
        {
            CannotApprove = 0,
            EmployerCanApprove = 1,
            ProviderCanApprove = 2,
            BothCanApprove = EmployerCanApprove | ProviderCanApprove,
        }

        [TestCaseSource(nameof(MissingPropertiesTestData))]
        public async Task Handle_WithApprenticeDetails_ShouldReturnExpectedEmployerCanApprove(DraftApprenticeshipDetails apprenticeship, AllowedApproval allowedApproval)
        {
            await CheckQueryResponse(
                response => response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove)),
                apprenticeship);
        }

        [TestCaseSource(nameof(MissingPropertiesTestData))]
        public async Task Handle_WithApprenticeDetails_ShouldReturnExpectedProviderCanApprove(DraftApprenticeshipDetails apprenticeship, AllowedApproval allowedApproval)
        {
            await CheckQueryResponse(
                response => response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove)),
                apprenticeship);
        }

        [TestCase("email@example.com", false, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", true, AllowedApproval.BothCanApprove)]
        [TestCase(null, false, AllowedApproval.BothCanApprove)]
        [TestCase(null, true, AllowedApproval.CannotApprove)]
        public async Task Handle_WithApprenticeEmail_ShouldReturnExpectedEmployerCanApprove(string email, bool apprenticeEmailRequired, AllowedApproval allowedApproval)
        {
            Action<GetSupportCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.EmailOptionalService
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>()))
                    .Returns(apprenticeEmailRequired);
            });

            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.Email, email)
                .With(x => x.StartDate, new DateTime(2022, 08, 01))
                .With(x=>x.RecognisePriorLearning, false)
                .Create();

            await CheckQueryResponse(response =>
            {
                response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove));
            },
                apprenticeDetails, arrange);
        }

        [TestCase(false, AllowedApproval.BothCanApprove)]
        [TestCase(true, AllowedApproval.CannotApprove)]
        public async Task Handle_WithRplDetails_ShouldReturnExpectedProviderCanApproveStatus(bool recognisePriorLearning, AllowedApproval allowedApproval)
        {
            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, new DateTime(2022, 08, 01))
                .With(x => x.RecognisePriorLearning, false)
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                },
                apprenticeDetails, null, null, recognisePriorLearning);
        }

        [TestCase(false, AllowedApproval.BothCanApprove)]
        [TestCase(true, AllowedApproval.EmployerCanApprove)]
        public async Task Handle_WithRplDetails_ShouldReturnExpectedEmployerCanApproveStatus(bool recognisePriorLearning, AllowedApproval allowedApproval)
        {
            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, new DateTime(2022, 08, 01))
                .With(x => x.RecognisePriorLearning, false)
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove));
                },
                apprenticeDetails, null, null, recognisePriorLearning);
        }

        [TestCase(null, null, AllowedApproval.CannotApprove)]
        [TestCase(null, 101, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", null, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", 101, AllowedApproval.BothCanApprove)]
        public async Task Handle_WithApprenticeEmailAndAContinuationOfId_ShouldReturnExpectedEmployerCanApprove(string email, long? continuationOfId, AllowedApproval allowedApproval)
        {
            Action<GetSupportCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.EmailOptionalService
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            });

            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.Email, email)
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                    response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove));
                },
                apprenticeDetails, arrange, continuationOfId);
        }

        [TestCase(ApprenticeshipEmployerType.Levy)]
        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedLevyStatus(
            ApprenticeshipEmployerType levyStatus)
        {
            LevyStatus = levyStatus;
            await CheckQueryResponse(response =>
                Assert.AreEqual(LevyStatus, response.LevyStatus, "Did not return expected LevyStatus"));
        }

        [TestCase(null)]
        [TestCase(123)]
        public async Task Handle_CohortWithChangeOfParty_ShouldReturnChangeOfPartyRequestId(long? value)
        {
            ChangeOfPartyRequestId = value;
            await CheckQueryResponse(response => Assert.AreEqual(value, response.ChangeOfPartyRequestId));
        }

        private async Task CheckQueryResponse(Action<GetSupportCohortSummaryQueryResult> assert,
            DraftApprenticeshipDetails apprenticeshipDetails = null,
            Action<GetSupportCohortSummaryHandlerTestFixtures> arrange = null, long? continuationOfId = null, bool? recognisePriorLearning = false)
        {
            var autoFixture = new Fixture();

            var account = new Account(autoFixture.Create<long>(), "", "", "", DateTime.UtcNow)
            { LevyStatus = LevyStatus };
            AccountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", autoFixture.Create<string>(),
                OrganisationType.Charities, "", DateTime.UtcNow);
            Provider = new Provider { Name = autoFixture.Create<string>() };

            CohortId = autoFixture.Create<long>();
            AccountId = autoFixture.Create<long>();
            Cohort = autoFixture.Build<Cohort>().Without(o => o.Apprenticeships).Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity).Without(o => o.Provider).Without(o => o.TransferSender)
                .With(o => o.WithParty, Party.Provider)
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
                draftApprenticeship.RecognisePriorLearning = recognisePriorLearning;
                Cohort.Apprenticeships.Add(draftApprenticeship);
            }

            // arrange
            var fixtures = new GetSupportCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, Cohort, WithParty, LatestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider, Approvals, ChangeOfPartyRequestId);

            arrange?.Invoke(fixtures);

            // act
            var response = await fixtures.GetResult(new GetSupportCohortSummaryQuery(CohortId, AccountId));

            // Assert
            assert(response);
        }
    }

    public class GetSupportCohortSummaryHandlerTestFixtures
    {
        public GetSupportCohortSummaryHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetSupportCohortSummaryQuery, GetSupportCohortSummaryQueryResult>>();
            ValidatorMock = new Mock<IValidator<GetSupportCohortSummaryQuery>>();
            EmailOptionalService = new Mock<IEmailOptionalService>();
            SeedCohorts = new List<Cohort>();
            _mapper = new Mock<IMapper<Apprenticeship, SupportApprenticeshipDetails>>();
        }

        public Mock<IRequestHandler<GetSupportCohortSummaryQuery, GetSupportCohortSummaryQueryResult>> HandlerMock { get; set; }

        public IRequestHandler<GetSupportCohortSummaryQuery, GetSupportCohortSummaryQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetSupportCohortSummaryQuery>> ValidatorMock { get; set; }
        public Mock<IEmailOptionalService> EmailOptionalService { get; set; }
        public IValidator<GetSupportCohortSummaryQuery> Validator => ValidatorMock.Object;
        private Mock<IMapper<Apprenticeship, SupportApprenticeshipDetails>> _mapper;

        public List<Cohort> SeedCohorts { get; }

        public GetSupportCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, Cohort cohort, Party withParty,
            string latestMessageCreatedByEmployer, string latestMessageCreatedByProvider, Party approvals,
            long? changeOfPartyRequestId)
        {
            cohort.Id = cohortId;
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

        public Task<GetSupportCohortSummaryQueryResult> GetResult(GetSupportCohortSummaryQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetSupportCohortSummaryHandler(lazy, _mapper.Object, EmailOptionalService.Object);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(databaseName: "SFA.DAS.Commitments.Database")
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            using var dbContext = new ProviderCommitmentsDbContext(options);
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
            return action(dbContext);
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Cohorts.AddRange(SeedCohorts);
            dbContext.SaveChanges(true);
        }

        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}