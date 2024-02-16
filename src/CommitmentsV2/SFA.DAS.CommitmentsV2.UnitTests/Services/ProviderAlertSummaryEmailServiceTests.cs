using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class ProviderAlertSummaryEmailServiceTests
{

    private ProviderAlertSummaryEmailsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ProviderAlertSummaryEmailsFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture.TearDown();
        _fixture = null;
    }

    [Test]
    public async Task AndNoSummariesFound_ThenShouldNotCallSendEmailToAllProviderRecipients()
    {
        await _fixture.Sut.SendAlertSummaryEmails(_fixture.JobId);
        _fixture.VerifySendEmailToAllProviderRecipientsIsNeverCalled();
    }

    [Test]
    public async Task AndOneSummaryFound_ThenShouldCallSendEmailToAllProviderRecipientsOnce()
    {
        var fixture = _fixture.WithProviderOneSummaryAlert(_fixture.FirstProviderId);
        await fixture.Sut.SendAlertSummaryEmails(fixture.JobId);
        fixture.VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(fixture.FirstProviderId, 1, 1);
    }

    [Test]
    public async Task AndDifferentSummariesFound_ThenShouldCallSendEmailToAllProviderRecipientsOnceForEachProvider()
    {
        var fixture = _fixture.WithMultipleProviderSummaryAlerts();
        await fixture.Sut.SendAlertSummaryEmails(fixture.JobId);
        fixture.VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert();
    }

}

public class ProviderAlertSummaryEmailsFixture
{
    public ProviderAlertSummaryEmailService Sut;
    public string JobId;
    public long FirstProviderId;
    public long SecondProviderId;
    public long AccountId;
    public string LegalEntityIdentifier { get; }
    public long FirstApprenticeshipId;
    public long NoApprenticeChangesApprenticeshipId;
    public long NoDataLocksApprenticeshipId;
    public List<Apprenticeship> SeedApprenticeships { get; }
    private Fixture Fixture = new Fixture();
    public ProviderCommitmentsDbContext Db { get; set; }
    private Mock<IMessageSession> _mockNserviceBusContext;
    private static CommitmentsV2Configuration commitmentsV2Configuration;
    private readonly string ProviderCommitmentsBaseUrl = "https://approvals.ResourceEnvironmentName-pas.apprenticeships.education.gov.uk/";        

    public ProviderAlertSummaryEmailsFixture()
    {
        JobId = Fixture.Create<string>();
        FirstProviderId = Fixture.Create<long>();
        SecondProviderId = Fixture.Create<long>();
        AccountId = Fixture.Create<long>();

        FirstApprenticeshipId = Fixture.Create<long>();
        NoApprenticeChangesApprenticeshipId = Fixture.Create<long>();
        NoDataLocksApprenticeshipId = Fixture.Create<long>();

        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning))
            .Options);

        _mockNserviceBusContext = new Mock<IMessageSession>();

        commitmentsV2Configuration = new CommitmentsV2Configuration()
        {
            ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl
        };

        Sut = new ProviderAlertSummaryEmailService(Db, Mock.Of<ILogger<ProviderAlertSummaryEmailService>>(), commitmentsV2Configuration, _mockNserviceBusContext.Object);
    }

    internal void VerifySendEmailToAllProviderRecipientsIsNeverCalled()
    {
        _mockNserviceBusContext.Verify(x => x.Send(It.IsAny<SendEmailToProviderCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    internal ProviderAlertSummaryEmailsFixture WithProviderOneSummaryAlert(long ukprn)
    {
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var provider = new Provider()
            .Set(c => c.UkPrn, ukprn)
            .Set(c => c.Name, $"Provider-{ukprn}")
            .Set(c => c.Created, DateTime.Today);

        var accountLegalEntity = new AccountLegalEntity()
            .Set(a => a.LegalEntityId, LegalEntityIdentifier)
            .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
            .Set(a => a.AccountId, AccountId)
            .Set(a => a.Id, Fixture.Create<long>());

        var cohort = new Cohort()
            .Set(c => c.Id, Fixture.Create<long>())
            .Set(c => c.EmployerAccountId, AccountId)
            .Set(c => c.ProviderId, ukprn)
            .Set(c => c.AccountLegalEntity, accountLegalEntity)
            .Set(c => c.Provider, provider);

        var apprenticeship = Fixture.Build<Apprenticeship>()
            .With(s => s.Id, Fixture.Create<long>())
            .With(s => s.Cohort, cohort)
            .With(s => s.EndDate, DateTime.UtcNow.AddYears(1))
            .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
            .Without(s => s.DataLockStatus)
            .Without(s => s.EpaOrg)
            .Without(s => s.ApprenticeshipUpdate)
            .Without(s => s.Continuation)
            .Without(s => s.PreviousApprenticeship)
            .Without(s => s.CompletionDate)
            .Without(s => s.EmailAddressConfirmed)
            .Without(s => s.ApprenticeshipConfirmationStatus)
            .Create();

        apprenticeship.DataLockStatus = SetupDataLocks(1);
        apprenticeship.PaymentStatus = PaymentStatus.Active;
        apprenticeship.PendingUpdateOriginator = Originator.Employer;

        Db.Apprenticeships.Add(apprenticeship);
        Db.SaveChanges();

        var x = Db.Apprenticeships.FirstAsync().Result;


        return this;
    }

    private static ICollection<DataLockStatus> SetupDataLocks(long apprenticeshipId)
    {
        var activeDataLock4 = new DataLockStatus
        {
            ApprenticeshipId = apprenticeshipId,
            EventStatus = EventStatus.New,
            IsExpired = false,
            TriageStatus = TriageStatus.Restart,
            ErrorCode = DataLockErrorCode.Dlock04
        };

        var activeDataLock5 = new DataLockStatus
        {
            ApprenticeshipId = apprenticeshipId,
            EventStatus = EventStatus.New,
            IsExpired = false,
            TriageStatus = TriageStatus.Restart,
            ErrorCode = DataLockErrorCode.Dlock05
        };

        var inactiveDataLock6 = new DataLockStatus
        {
            ApprenticeshipId = apprenticeshipId,
            EventStatus = EventStatus.Removed,
            IsExpired = false,
            TriageStatus = TriageStatus.Restart,
            ErrorCode = DataLockErrorCode.Dlock04
        };

        var dataLockForApprenticeshipBeforeStart = new DataLockStatus
        {
            ApprenticeshipId = apprenticeshipId,
            EventStatus = EventStatus.New,
            IsExpired = false,
            TriageStatus = TriageStatus.Change,
            ErrorCode = DataLockErrorCode.Dlock04
        };

        return new List<DataLockStatus> { activeDataLock4, activeDataLock5, inactiveDataLock6, dataLockForApprenticeshipBeforeStart };
    }

    internal ProviderAlertSummaryEmailsFixture WithMultipleProviderSummaryAlerts()
    {
        WithProviderOneSummaryAlert(FirstProviderId);
        WithProviderOneSummaryAlert(SecondProviderId);

        return this;
    }

    internal void VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(long providerId, int changesForReview, int dataMismatchCount)
    {
        VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(providerId,  changesForReview,  dataMismatchCount, 1);
    }

    internal void VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert()
    {
        VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(FirstProviderId, 1, 1, 2);
        VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(SecondProviderId, 1, 1, 2);
    }

    internal void VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(long providerId, int changesForReview, int dataMismatchCount, int n)
    {
        _mockNserviceBusContext
            .Verify(x => x.Send(It.Is<SendEmailToProviderCommand>(p => p.Template == "ProviderAlertSummaryNotification2" &&
                                                                       ValidateTokens(p.Tokens, changesForReview, dataMismatchCount)), It.IsAny<SendOptions>()),
                Times.Exactly(n));
    }

    private static bool ValidateTokens(IReadOnlyDictionary<string, string> tokens, int changesForReview, int dataMismatchCount)
    {
        return tokens["total_count_text"] == (changesForReview + dataMismatchCount).ToString()
               && tokens["link_to_mange_apprenticeships"].StartsWith(commitmentsV2Configuration.ProviderCommitmentsBaseUrl)
               && changesForReview == 0 ?
            string.IsNullOrWhiteSpace(tokens["changes_for_review"]) :
            tokens["changes_for_review"].StartsWith("* " + changesForReview)
            && dataMismatchCount == 0 ?
                string.IsNullOrWhiteSpace(tokens["mismatch_changes"]) :
                tokens["mismatch_changes"].StartsWith("* " + dataMismatchCount);
    }

    public void TearDown()
    {
        Db.Database.EnsureDeleted();
    }
}