using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class ProcessFullyApprovedCohortCommandHandlerTests
{
    [TestCase(ApprenticeshipEmployerType.NonLevy)]
    [TestCase(ApprenticeshipEmployerType.Levy)]
    public void Handle_WhenHandlingCommand_ThenShouldProcessFullyApprovedCohort(ApprenticeshipEmployerType apprenticeshipEmployerType)
    {
        var fixture = new ProcessFullyApprovedCohortCommandFixture();
        fixture.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
            .Handle();
            
        fixture.Db.Verify(d => d.ExecuteSqlCommandAsync(
                "EXEC ProcessFullyApprovedCohort @cohortId, @accountId, @apprenticeshipEmployerType",
                It.Is<SqlParameter>(p => p.ParameterName == "cohortId" && p.Value.Equals(fixture.Command.CohortId)),
                It.Is<SqlParameter>(p => p.ParameterName == "accountId" && p.Value.Equals(fixture.Command.AccountId)),
                It.Is<SqlParameter>(p => p.ParameterName == "apprenticeshipEmployerType" && p.Value.Equals(apprenticeshipEmployerType))),
            Times.Once);
    }
        
    [TestCase(ApprenticeshipEmployerType.NonLevy, false)]
    [TestCase(ApprenticeshipEmployerType.NonLevy, true)]
    [TestCase(ApprenticeshipEmployerType.Levy, false)]
    [TestCase(ApprenticeshipEmployerType.Levy, true)]
    public void Handle_WhenHandlingCommand_ThenShouldPublishEvents(ApprenticeshipEmployerType apprenticeshipEmployerType, bool isFundedByTransfer)
    {
        var fixture = new ProcessFullyApprovedCohortCommandFixture();
        fixture.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
            .SetApprovedApprenticeships(isFundedByTransfer)
            .Handle();
            
        fixture.Apprenticeships.ForEach(
            a => fixture.EventPublisher.Verify(
                p => p.Publish(It.Is<ApprenticeshipCreatedEvent>(
                    e => ProcessFullyApprovedCohortCommandFixture.IsValid(apprenticeshipEmployerType, a, e))),
                Times.Once));
    }


    [Test]
    public void Handle_WhenHandlingCommand_WithChangeOfParty_ThenShouldPublishApprenticeshipWithChangeOfPartyCreatedEvents()
    {
        var fixture = new ProcessFullyApprovedCohortCommandFixture();
        fixture.SetChangeOfPartyRequest(true)
            .SetApprenticeshipEmployerType(ApprenticeshipEmployerType.NonLevy)
            .SetApprovedApprenticeships(false)
            .Handle();

        fixture.Apprenticeships.ForEach(
            a => fixture.EventPublisher.Verify(
                p => p.Publish(It.Is<ApprenticeshipWithChangeOfPartyCreatedEvent>(
                    e => fixture.IsValidChangeOfPartyEvent(a, e))),
                Times.Once));
    }

    [Test]
    public void Handle_WhenHandlingCommand_WithChangeOfParty_ThenShouldAddContinuationOfIdToApprenticeCreatedEvents()
    {
        var fixture = new ProcessFullyApprovedCohortCommandFixture();
        fixture.SetChangeOfPartyRequest(true)
            .SetApprenticeshipEmployerType(ApprenticeshipEmployerType.NonLevy)
            .SetApprovedApprenticeshipAsContinuation()
            .Handle();

            fixture.Apprenticeships.ForEach(
                a => fixture.EventPublisher.Verify(
                    p => p.Publish(It.Is<ApprenticeshipCreatedEvent>(
                        e => e.ContinuationOfId == fixture.PreviousApprenticeshipId)),
                    Times.Once));
        }
    }

    public class ProcessFullyApprovedCohortCommandFixture
    {
        public IFixture AutoFixture { get; set; }
        public ProcessFullyApprovedCohortCommand Command { get; set; }
        public Mock<IAccountApiClient> AccountApiClient { get; set; }
        public Mock<ProviderCommitmentsDbContext> Db { get; set; }
        public Mock<IEventPublisher> EventPublisher { get; set; }
        public Mock<IEncodingService> EncodingService { get; set; }
        public List<Apprenticeship> Apprenticeships { get; set; }
        public IRequestHandler<ProcessFullyApprovedCohortCommand> Handler { get; set; }
        public long PreviousApprenticeshipId { get; set; }
        public string ExpectedApprenticeshipHashedId { get; set; }
        


        public ProcessFullyApprovedCohortCommandFixture()
        {
            AutoFixture = new Fixture();
            EncodingService = new Mock<IEncodingService>();
            ExpectedApprenticeshipHashedId = AutoFixture.Create<string>();
            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), It.IsAny<EncodingType>())).Returns(ExpectedApprenticeshipHashedId);
            Command = AutoFixture.Create<ProcessFullyApprovedCohortCommand>();
            Command.SetValue(x => x.ChangeOfPartyRequestId, default(long?));
            AccountApiClient = new Mock<IAccountApiClient>();
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            EventPublisher = new Mock<IEventPublisher>();
            Apprenticeships = new List<Apprenticeship>();
            Handler = new ProcessFullyApprovedCohortCommandHandler(AccountApiClient.Object, new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), EventPublisher.Object, EncodingService.Object, Mock.Of<ILogger<ProcessFullyApprovedCohortCommandHandler>>());
            
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        Db.Setup(d => d.ExecuteSqlCommandAsync(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task.CompletedTask);
        EventPublisher.Setup(p => p.Publish(It.IsAny<object>())).Returns(Task.CompletedTask);
        PreviousApprenticeshipId = AutoFixture.Create<long>();
    }

    public Task Handle()
    {
        return Handler.Handle(Command, CancellationToken.None);
    }

    public ProcessFullyApprovedCohortCommandFixture SetApprenticeshipEmployerType(ApprenticeshipEmployerType apprenticeshipEmployerType)
    {
        AccountApiClient.Setup(c => c.GetAccount(Command.AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = apprenticeshipEmployerType.ToString()
            });
            
        return this;
    }

    public ProcessFullyApprovedCohortCommandFixture SetChangeOfPartyRequest(bool isChangeOfParty)
    {
        Command.SetValue(x => x.ChangeOfPartyRequestId, isChangeOfParty ? 123 : default(long?));
        return this;
    }

    public ProcessFullyApprovedCohortCommandFixture SetApprovedApprenticeships(bool isFundedByTransfer)
    {
        var provider = new Provider {Name = "Test Provider"};
        var account = new Account(1, "", "", "", DateTime.UtcNow);
        var accountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", "Test Employer", OrganisationType.Charities, "", DateTime.UtcNow);

        AutoFixture.Inject(account);

        var cohortBuilder = AutoFixture.Build<Cohort>()
            .Without(c => c.Apprenticeships)
            .With(c => c.AccountLegalEntity, accountLegalEntity)
            .With(c => c.Provider, provider)
            .With(x => x.IsDeleted, false);

        if (!isFundedByTransfer)
        {
            cohortBuilder.Without(c => c.TransferSenderId).Without(c => c.TransferApprovalActionedOn);
        }

        var apprenticeshipBuilder = AutoFixture.Build<Apprenticeship>()
            .Without(a => a.DataLockStatus)
            .Without(a => a.EpaOrg)
            .Without(a => a.ApprenticeshipUpdate)
            .Without(a => a.Continuation)
            .Without(s => s.ApprenticeshipConfirmationStatus)
            .Without(a => a.PreviousApprenticeship);

        var cohort1 = cohortBuilder.With(c => c.Id, Command.CohortId).Create();
        var cohort2 = cohortBuilder.Create();
            
        var apprenticeship1 = apprenticeshipBuilder.With(a => a.Cohort, cohort1).Create(); 
        var apprenticeship2 = apprenticeshipBuilder.With(a => a.Cohort, cohort1).Create();
        var apprenticeship3 = apprenticeshipBuilder.With(a => a.Cohort, cohort2).Create();
            
        var apprenticeships1 = new[] { apprenticeship1, apprenticeship2 };
        var apprenticeships2 = new[] { apprenticeship1, apprenticeship2, apprenticeship3 };
            
        Apprenticeships.AddRange(apprenticeships1);
        Db.Object.AccountLegalEntities.Add(accountLegalEntity);
        Db.Object.Providers.Add(provider);
        Db.Object.Apprenticeships.AddRange(apprenticeships2);

        Db.Object.SaveChanges();
            
        return this;
    }

    public ProcessFullyApprovedCohortCommandFixture SetApprovedApprenticeshipAsContinuation()
    {
        var provider = new Provider { Name = "Test Provider" };
        var account = new Account(1, "", "", "", DateTime.UtcNow);
        var accountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", "Test Employer", OrganisationType.Charities, "", DateTime.UtcNow);

        AutoFixture.Inject(account);
            
        var cohortBuilder = AutoFixture.Build<Cohort>()
            .Without(c => c.Apprenticeships)
            .With(c => c.AccountLegalEntity, accountLegalEntity)
            .With(c => c.Provider, provider)
            .With(x => x.IsDeleted, false)
            .Without(c => c.TransferSenderId).Without(c => c.TransferApprovalActionedOn);

        var apprenticeshipBuilder = AutoFixture.Build<Apprenticeship>()
            .Without(a => a.DataLockStatus)
            .Without(a => a.EpaOrg)
            .Without(a => a.ApprenticeshipUpdate)
            .Without(a => a.Continuation)
            .Without(a => a.PreviousApprenticeship);

        var cohort = cohortBuilder.With(c => c.Id, Command.CohortId).Create();

        var apprenticeshipNew = apprenticeshipBuilder
            .With(a => a.Cohort, cohort)
            .With(a => a.ContinuationOfId, PreviousApprenticeshipId)
            .Without(s => s.ApprenticeshipConfirmationStatus)
            .Create();

        var apprenticeships = new[] { apprenticeshipNew };

        Db.Object.AccountLegalEntities.Add(accountLegalEntity);
        Db.Object.Providers.Add(provider);
        Db.Object.Apprenticeships.AddRange(apprenticeships);

        Db.Object.SaveChanges();

        return this;
    }

    public static bool IsValid(ApprenticeshipEmployerType apprenticeshipEmployerType, Apprenticeship apprenticeship, ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var isValid = apprenticeshipCreatedEvent.ApprenticeshipId == apprenticeship.Id &&
                      apprenticeshipCreatedEvent.CreatedOn.Date == DateTime.UtcNow.Date &&
                      apprenticeshipCreatedEvent.AgreedOn == apprenticeship.Cohort.EmployerAndProviderApprovedOn &&
                      apprenticeshipCreatedEvent.AccountId == apprenticeship.Cohort.EmployerAccountId &&
                      apprenticeshipCreatedEvent.AccountLegalEntityPublicHashedId == apprenticeship.Cohort.AccountLegalEntity.PublicHashedId &&
                      apprenticeshipCreatedEvent.LegalEntityName == apprenticeship.Cohort.AccountLegalEntity.Name &&
                      apprenticeshipCreatedEvent.ProviderId == apprenticeship.Cohort.Provider.UkPrn &&
                      apprenticeshipCreatedEvent.TransferSenderId == apprenticeship.Cohort.TransferSenderId &&
                      apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval == apprenticeshipEmployerType &&
                      apprenticeshipCreatedEvent.Uln == apprenticeship.Uln &&
                      apprenticeshipCreatedEvent.TrainingType == apprenticeship.ProgrammeType.Value &&
                      apprenticeshipCreatedEvent.TrainingCode == apprenticeship.CourseCode &&
                      apprenticeshipCreatedEvent.DeliveryModel == apprenticeship.DeliveryModel &&
                      apprenticeshipCreatedEvent.StartDate == apprenticeship.StartDate.Value &&
                      apprenticeshipCreatedEvent.EndDate == apprenticeship.EndDate.Value &&
                      apprenticeshipCreatedEvent.PriceEpisodes.Length == apprenticeship.PriceHistory.Count &&
                      apprenticeshipCreatedEvent.DateOfBirth == apprenticeship.DateOfBirth &&
                      apprenticeshipCreatedEvent.ActualStartDate == apprenticeship.ActualStartDate &&
                      apprenticeshipCreatedEvent.FirstName == apprenticeship.FirstName &&
                      apprenticeshipCreatedEvent.LastName == apprenticeship.LastName &&
                      apprenticeshipCreatedEvent.LearnerDataId == apprenticeship.LearnerDataId;


        for (var index = 0; index < apprenticeship.PriceHistory.Count; index++)
        {
            var priceHistory = apprenticeship.PriceHistory.ElementAt(index);
            var priceEpisode = apprenticeshipCreatedEvent.PriceEpisodes.ElementAtOrDefault(index);

            isValid = isValid &&
                      priceEpisode?.FromDate == priceHistory.FromDate &
                      priceEpisode?.ToDate == priceHistory.ToDate &
                      priceEpisode?.Cost == priceHistory.Cost &
                      priceEpisode?.TrainingPrice == priceHistory.TrainingPrice &
                      priceEpisode.EndPointAssessmentPrice == priceHistory.AssessmentPrice;
        }
            
        return isValid;
    }

    public static bool IsValidCostBreakdown(Apprenticeship apprenticeship, ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var priceEpisode = apprenticeshipCreatedEvent.PriceEpisodes.First();
        return priceEpisode.TrainingPrice == apprenticeship.TrainingPrice && priceEpisode.EndPointAssessmentPrice == apprenticeship.EndPointAssessmentPrice;
    }

    public bool IsValidChangeOfPartyEvent(Apprenticeship apprenticeship, ApprenticeshipWithChangeOfPartyCreatedEvent changeOfPartyCreatedEvent)
    {
        return apprenticeship.Id == changeOfPartyCreatedEvent.ApprenticeshipId
               && Command.ChangeOfPartyRequestId == changeOfPartyCreatedEvent.ChangeOfPartyRequestId;
    }
}