using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.External.Messages.Events.HiringGrant;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.UnitTests.EventHandlers;

[TestFixture]
public class RequestingPreEmploymentVerificationEventHandlerTests
{
    private static readonly DateTime Now = new(2026, 11, 17, 11, 0, 0, DateTimeKind.Utc);
    private ProviderCommitmentsDbContext _db;
    private RequestingPreEmploymentVerificationEventHandler _handler;
    private RequestingPreEmploymentVerification _message;

    [SetUp]
    public void SetUp()
    {
        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), options => options.EnableNullChecks(false))
            .Options);
        _handler = new RequestingPreEmploymentVerificationEventHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            Mock.Of<ICurrentDateTime>(clock => clock.UtcNow == Now),
            Mock.Of<ILogger<RequestingPreEmploymentVerificationEventHandler>>());
        _message = new RequestingPreEmploymentVerification
        {
            ApprenticeshipId = 2321,
            UKPRN = 10012345,
            ULN = 3234567890,
            EmployerAccountIdentifier = 322131
        };
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task Handle_WhenNoHiringPaymentExists_SavesCheckingStatusAndEventIdentifiers()
    {
        await SeedApprenticeship();

        await HandleAndSave();

        var saved = await _db.HiringPayments.SingleAsync();
        saved.ApprenticeshipId.Should().Be(_message.ApprenticeshipId);
        ((short)saved.Status).Should().Be(1);
        saved.Created.Should().Be(Now);
        saved.Updated.Should().BeNull();
        saved.Ukprn.Should().Be(_message.UKPRN);
        saved.Uln.Should().Be("3234567890");
        saved.EmployerAccountId.Should().Be(_message.EmployerAccountIdentifier);
    }

    [Test]
    public async Task Handle_WhenEventIdentifiersAreMissing_UsesApprenticeshipIdentifiers()
    {
        var apprenticeship = await SeedApprenticeship();
        _message.UKPRN = null;
        _message.ULN = null;
        _message.EmployerAccountIdentifier = null;

        await HandleAndSave();

        var saved = await _db.HiringPayments.SingleAsync();
        saved.Ukprn.Should().Be(apprenticeship.Cohort.ProviderId);
        saved.Uln.Should().Be(apprenticeship.Uln);
        saved.EmployerAccountId.Should().Be(apprenticeship.Cohort.EmployerAccountId);
    }

    [Test]
    public async Task Handle_WhenHiringPaymentExists_ResetsStatusAndUpdatesTimestamp([Values] HiringPaymentStatus status)
    {
        await SeedApprenticeship();
        var existing = new HiringPayment
        {
            ApprenticeshipId = _message.ApprenticeshipId,
            Status = status,
            Created = Now.AddDays(-2),
            Updated = Now.AddDays(-1),
            Ukprn = 10009876,
            Uln = "1234567890",
            EmployerAccountId = 400001
        };
        _db.HiringPayments.Add(existing);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        await HandleAndSave();

        var saved = await _db.HiringPayments.SingleAsync();
        saved.ApprenticeshipId.Should().Be(existing.ApprenticeshipId);
        ((short)saved.Status).Should().Be(1);
        saved.Updated.Should().Be(Now);
        saved.Created.Should().Be(existing.Created);
        saved.Ukprn.Should().Be(existing.Ukprn);
        saved.Uln.Should().Be(existing.Uln);
        saved.EmployerAccountId.Should().Be(existing.EmployerAccountId);
    }

    [Test]
    public void Handle_WhenApprenticeshipDoesNotExist_FailsWithoutAddingHiringPayment()
    {
        Assert.ThrowsAsync<DomainException>(HandleAndSave);
        _db.HiringPayments.Should().BeEmpty();
    }

    private async Task<Apprenticeship> SeedApprenticeship()
    {
        var apprenticeship = new Apprenticeship
        {
            Id = _message.ApprenticeshipId,
            Uln = "9123456789",
            Cohort = new Cohort
            {
                Id = 1,
                ProviderId = 10054321,
                EmployerAccountId = 123456
            }
        };
        _db.Apprenticeships.Add(apprenticeship);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        return apprenticeship;
    }

    private async Task HandleAndSave()
    {
        await _handler.Handle(_message, Mock.Of<IMessageHandlerContext>());
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
    }
}
