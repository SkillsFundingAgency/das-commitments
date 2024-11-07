using AutoFixture.Dsl;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
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
            await CheckQueryResponse(response => Assert.That(response, Is.Not.Null, "Did not return response"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortId()
        {
            await CheckQueryResponse(response =>
                Assert.That(response.CohortId, Is.EqualTo(CohortId), "Did not return expected cohort id"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedCohortReference()
        {
            await CheckQueryResponse(response => Assert.That(response.CohortReference, Is.EqualTo(Cohort.Reference),
                "Did not return expected cohort reference"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedProviderName()
        {
            await CheckQueryResponse(response =>
                Assert.That(response.ProviderName, Is.EqualTo(Provider.Name), "Did not return expected provider name"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLegalEntityName()
        {
            await CheckQueryResponse(response => Assert.That(response.LegalEntityName, Is.EqualTo(AccountLegalEntity.Name),
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
                Assert.That(response.WithParty, Is.EqualTo(expectedParty), "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithTransferSender_ShouldReturnTransferSender()
        {
            WithParty = Party.TransferSender;
            HasTransferSender = false;
            await CheckQueryResponse(response =>
                Assert.That(response.WithParty, Is.EqualTo(Party.TransferSender), "Did not return expected Party type"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByEmployer()
        {
            await CheckQueryResponse(response => Assert.That(response.LatestMessageCreatedByEmployer, Is.EqualTo(LatestMessageCreatedByEmployer), "Did not return expected latest message created by employer"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLatestMessageCreatedByProvider()
        {
            await CheckQueryResponse(response => Assert.That(response.LatestMessageCreatedByProvider, Is.EqualTo(LatestMessageCreatedByProvider), "Did not return expected latest message created by provider"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityPublicHashedId()
        {
            await CheckQueryResponse(response => Assert.That(response.AccountLegalEntityPublicHashedId, Is.EqualTo(Cohort.AccountLegalEntity.PublicHashedId), "Did not return expected account legal entity public hashed ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedAccountLegalEntityId()
        {
            await CheckQueryResponse(response => Assert.That(response.AccountLegalEntityId, Is.EqualTo(AccountLegalEntity.Id), "Did not return expected account legal entity ID"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastAction()
        {
            await CheckQueryResponse(response =>
                Assert.That(response.LastAction, Is.EqualTo(Cohort.LastAction), "Did not return expected Last Action"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByEmployerEmail()
        {
            await CheckQueryResponse(response => Assert.That(response.LastUpdatedByEmployerEmail, Is.EqualTo(Cohort.LastUpdatedByEmployerEmail), "Did not return expected Last Updated By Employer Email"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedEmployerAccountId()
        {
            await CheckQueryResponse(response => Assert.That(response.AccountId, Is.EqualTo(Cohort.EmployerAccountId), "Did not return expected EmployerAccountId"));
        }

        [Test]
        public async Task Handle_WithSpecifiedId_ShouldReturnExpectedLastUpdatedByProviderEmail()
        {
            await CheckQueryResponse(response => Assert.That(response.LastUpdatedByProviderEmail, Is.EqualTo(Cohort.LastUpdatedByProviderEmail), "Did not return expected Last Updated By Provider Email"));
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, true)]
        [TestCase(Party.Employer | Party.Provider, true)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedIsApprovedByEmployer(Party approvals, bool expectIsApprovedByEmployer)
        {
            WithParty = Party.Employer;
            Approvals = approvals;
            await CheckQueryResponse(response => Assert.That(response.IsApprovedByEmployer, Is.EqualTo(expectIsApprovedByEmployer), "Did not return expected IsApprovedByEmployer"));
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
                        .With(x=>x.StartDate, new DateTime(2022,07,01))
                        .With(x => x.Email, "person@example.com");

                    yield return (AllowedApproval.BothCanApprove, completeApprenticeship);
                    yield return (AllowedApproval.BothCanApprove, completeApprenticeship.With(x => x.Email, (string)null));

                    yield return (AllowedApproval.EmployerCanApprove, completeApprenticeship.Without(x => x.Uln));

                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.FirstName));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.LastName));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.TrainingProgramme));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.Cost));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.EndDate));
                    yield return (AllowedApproval.CannotApprove, completeApprenticeship.Without(x => x.DateOfBirth));

                    var flexiPilotApprenticeship =
                        completeApprenticeship.With(x => x.IsOnFlexiPaymentPilot, true);
                    yield return (AllowedApproval.EmployerCanApprove, flexiPilotApprenticeship.Without(x => x.TrainingPrice));
                    yield return (AllowedApproval.EmployerCanApprove, flexiPilotApprenticeship.Without(x => x.EndPointAssessmentPrice));

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

        [Test]
        public async Task Handle_WithNoStartDateIsProvided_ShouldReturnFalse()
        {
            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, (DateTime?)null)
                .With(x => x.ActualStartDate, (DateTime?)null)
                 .With(x => x.EndDate, DateTime.Now.AddYears(1))
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().BeFalse();
                },
                apprenticeDetails);
        }

        [Test]
        public async Task Handle_WithEstimatedStartDateIsProvided_ShouldReturnTrue()
        {
            var startDate = new DateTime(2022, 07, 01);
            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, startDate)
                .With(x => x.ActualStartDate, (DateTime?)null)
                 .With(x => x.EndDate, startDate.AddYears(1))
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().BeTrue();
                },
                apprenticeDetails);
        }

        [Test]
        public async Task Handle_WithActualStartDateIsProvided_ShouldReturnTrue()
        {
            var startDate = new DateTime(2022, 07, 01);
            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, (DateTime?)null)
                .With(x => x.ActualStartDate, startDate)
                 .With(x => x.EndDate, startDate.AddYears(1))
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().BeTrue();
                },
                apprenticeDetails);
        }

        [TestCase("2022-07-01", null, null, null, null, null, null, AllowedApproval.BothCanApprove)]
        [TestCase("2022-08-01", null, null, null, null, null, null, AllowedApproval.CannotApprove)]
        [TestCase("2022-08-01", false, null, null, null, null, null, AllowedApproval.BothCanApprove)]
        [TestCase("2022-08-01", true, 1000, 100, false, null, 1000, AllowedApproval.BothCanApprove)]
        [TestCase("2022-08-01", true, 1000, null, false, null, 1000, AllowedApproval.CannotApprove)]
        [TestCase("2022-08-01", true, 1000, 100, null, null, 1000, AllowedApproval.CannotApprove)]
        [TestCase("2022-08-01", true, 1000, 100, true, null, 1000, AllowedApproval.CannotApprove)]
        [TestCase("2022-08-01", true, 10, 100, true, 12, null, AllowedApproval.CannotApprove)]
        public async Task Handle_WithApprenticeRPLExtendedConsidered_ShouldReturnExpectedProviderCanApprove(DateTime startDate, bool? recognisePriorLearning, int? trainingTotalHours,
            int? durationReducedByHours, bool? isDurationBeingReduced, int? durationReducedBy, int? priceReducedBy, AllowedApproval allowedApproval)
        {
            Action<GetCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.SetupRPLExtendedData(recognisePriorLearning, trainingTotalHours, durationReducedByHours, isDurationBeingReduced, durationReducedBy, priceReducedBy);
            });

            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, startDate)
                .With(x => x.EndDate, startDate.AddYears(1))
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                },
                apprenticeDetails, arrange);
        }

        [TestCase("email@example.com", false, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", true, AllowedApproval.BothCanApprove)]
        [TestCase(null, false, AllowedApproval.BothCanApprove)]
        [TestCase(null, true, AllowedApproval.CannotApprove)]
        public async Task Handle_WithApprenticeEmail_ShouldReturnExpectedEmployerCanApprove(string email, bool apprenticeEmailRequired, AllowedApproval allowedApproval)
        {
            Action<GetCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.EmailOptionalService
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>()))
                    .Returns(apprenticeEmailRequired);
            });

            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.Email, email)
                .With(x => x.StartDate, new DateTime(2022,7,1))
                .With(x => x.EndDate, new DateTime(2023,7,1))
                .Create();

            await CheckQueryResponse(response =>
            {
                response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove));
            },
                apprenticeDetails, arrange);
        }

        [TestCase(null, null, AllowedApproval.CannotApprove)]
        [TestCase(null, 101, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", null, AllowedApproval.BothCanApprove)]
        [TestCase("email@example.com", 101, AllowedApproval.BothCanApprove)]
        public async Task Handle_WithApprenticeEmailAndAContinuationOfId_ShouldReturnExpectedEmployerCanApprove(string email, long? continuationOfId, AllowedApproval allowedApproval)
        {
            Action<GetCohortSummaryHandlerTestFixtures> arrange = (f =>
            {
                f.EmailOptionalService
                    .Setup(x => x.ApprenticeEmailIsRequiredFor(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            });

            var apprenticeDetails = new Fixture()
                .Build<DraftApprenticeshipDetails>()
                .With(x => x.StartDate, new DateTime(2022, 07, 01))
                .With(x => x.Email, email)
                .Create();

            await CheckQueryResponse(response =>
                {
                    response.IsCompleteForProvider.Should().Be(allowedApproval.HasFlag(AllowedApproval.ProviderCanApprove));
                    response.IsCompleteForEmployer.Should().Be(allowedApproval.HasFlag(AllowedApproval.EmployerCanApprove));
                },
                apprenticeDetails, arrange, continuationOfId);
        }

        [Test]
        [NonParallelizable]
        public async Task Handle_DeletedCohort_ShouldReturnNull()
        {
            CohortIsDeleted = true;
            await CheckQueryResponse((x) => Assert.That(x, Is.Null));
        }

        [TestCase(ApprenticeshipEmployerType.Levy)]
        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        public async Task Handle_WithSpecifiedApprovals_ShouldReturnExpectedLevyStatus(
            ApprenticeshipEmployerType levyStatus)
        {
            LevyStatus = levyStatus;
            await CheckQueryResponse(response =>
                Assert.That(response.LevyStatus, Is.EqualTo(LevyStatus), "Did not return expected LevyStatus"));
        }

        [TestCase(null)]
        [TestCase(123)]
        public async Task Handle_CohortWithChangeOfParty_ShouldReturnChangeOfPartyRequestId(long? value)
        {
            ChangeOfPartyRequestId = value;
            await CheckQueryResponse(response => Assert.That(response.ChangeOfPartyRequestId, Is.EqualTo(value)));
        }

        private async Task CheckQueryResponse(Action<GetCohortSummaryQueryResult> assert,
            DraftApprenticeshipDetails apprenticeshipDetails = null,
            Action<GetCohortSummaryHandlerTestFixtures> arrange = null, long? continuationOfId = null)
        {
            var autoFixture = new Fixture();

            var account = new Account(autoFixture.Create<long>(), "", "", "", DateTime.UtcNow)
                {LevyStatus = LevyStatus};
            AccountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", autoFixture.Create<string>(),
                OrganisationType.Charities, "", DateTime.UtcNow);
            Provider = new Provider {Name = autoFixture.Create<string>()};

            CohortId = autoFixture.Create<long>();
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
                draftApprenticeship.Cohort = Cohort;
                draftApprenticeship.CommitmentId = CohortId;
                draftApprenticeship.ContinuationOfId = continuationOfId;
                Cohort.Apprenticeships.Add(draftApprenticeship);
            }

            // arrange
            var fixtures = new GetCohortSummaryHandlerTestFixtures()
                .AddCommitment(CohortId, Cohort, WithParty, LatestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider, Approvals, ChangeOfPartyRequestId);

            arrange?.Invoke(fixtures);

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
            EmailOptionalService = new Mock<IEmailOptionalService>();
            FeatureTogglesService = new Mock<IFeatureTogglesService<FeatureToggle>>();
            SeedCohorts = new List<Cohort>();
        }

        public Mock<IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>> HandlerMock { get; set; }

        public IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetCohortSummaryQuery>> ValidatorMock { get; set; }
        public Mock<IEmailOptionalService> EmailOptionalService { get; set; }
        public Mock<IFeatureTogglesService<FeatureToggle>> FeatureTogglesService { get; set; }
        public IValidator<GetCohortSummaryQuery> Validator => ValidatorMock.Object;

        public List<Cohort> SeedCohorts { get; }

        public GetCohortSummaryHandlerTestFixtures AddCommitment(long cohortId, Cohort cohort, Party withParty,
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

        public Task<GetCohortSummaryQueryResult> GetResult(GetCohortSummaryQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortSummaryQueryHandler(lazy, EmailOptionalService.Object);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(databaseName: "SFA.DAS.Commitments.Database", b => b.EnableNullChecks(false))
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

        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        //public GetCohortSummaryHandlerTestFixtures SetupRPLData(bool? recognisePriorLearning, int? durationReducedBy, int? priceReducedBy)
        //{
        //    var apprenticeship = SeedCohorts.First().Apprenticeships.First();
        //    apprenticeship.RecognisePriorLearning = recognisePriorLearning;
        //    apprenticeship.PriorLearning = new ApprenticeshipPriorLearning();
        //    apprenticeship.PriorLearning.DurationReducedBy = durationReducedBy;
        //    apprenticeship.PriorLearning.PriceReducedBy = priceReducedBy;

        //    return this;
        //}

        public GetCohortSummaryHandlerTestFixtures SetupRPLExtendedData(bool? recognisePriorLearning, int? trainingTotalHours, int? durationReducedByHours, bool? isDurationReduced, int? durationReducedBy, int? priceReducedBy)
        {
            var apprenticeship = SeedCohorts.First().Apprenticeships.First();
            apprenticeship.TrainingTotalHours = trainingTotalHours;
            apprenticeship.RecognisePriorLearning = recognisePriorLearning;
            apprenticeship.PriorLearning = new ApprenticeshipPriorLearning();
            apprenticeship.PriorLearning.DurationReducedByHours = durationReducedByHours;
            apprenticeship.PriorLearning.IsDurationReducedByRpl = isDurationReduced;
            apprenticeship.PriorLearning.DurationReducedBy = durationReducedBy;
            apprenticeship.PriorLearning.PriceReducedBy = priceReducedBy;

            return this;
        }
    }
}