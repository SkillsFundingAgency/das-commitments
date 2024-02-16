using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

using TestsFixture = RejectDataLockRequestChangesCommandHandlerTestsFixture;

[TestFixture]
[Parallelizable]
public class RejectDataLockRequestChangesCommandHandlerTests
{
    private TestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new TestsFixture();
    }
        
    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }
        
    [Test]
    public async Task ShouldNotRemoveTriageDataLock_WhenNoNewDataLockToProcess()
    {
        // Arrange
        _fixture.SeedData()
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId, 50, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddMonths(1), 2000, false, TriageStatus.Change, EventStatus.New, false, Status.Pass, DataLockErrorCode.Dlock07);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyDataLockTriage(40, TriageStatus.Change, "Should not update already resolved datalocks");
        _fixture.VerifyDataLockTriage(50, TriageStatus.Change, "Should not update passed datalocks");
    }

    [Test]
    public async Task ShouldNotPublishStateChanged_WhenNoNewDataLockToProcess()
    {
        // Arrange
        _fixture.SeedData()
            .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId, 50, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddMonths(1), 2000, false, TriageStatus.Change, EventStatus.New, false, Status.Pass, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId, 60, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime.AddMonths(2), 3000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyEntityStateChangedEventPublished(Times.Never);
    }

    [Test]
    public async Task ShouldNotResolveDataLock_WhenHasHadDataLockSuccessAndNewDataLocksAreNotPriceOnly()
    {
        // Arrange
        _fixture.SeedData()
            .WithHasHadDataLockSuccess(true)
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(
                TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, 
                TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyDataLockTriage(40, TriageStatus.Change, "Should not update course/price datalocks when apprenticeship HasHadDataLockSuccess");
    }

    [Test]
    public async Task ShouldResolveDataLock_WhenHasHadDataLockSuccessAndNewDataLocksArePriceOnlyAndNotExpired()
    {
        // Arrange
        _fixture.SeedData()
            .WithHasHadDataLockSuccess(true)
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, true, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, true, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, true, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(
                TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false,
                TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyDataLockTriage(40, TriageStatus.Unknown, "Should not update course/price datalocks when apprenticeship HasHadDataLockSuccess");
    }

    [Test]
    public async Task ShouldNotPublishStateChanged_WhenHasHadDataLockSuccessAndNewDataLocksAreNotPriceOnly()
    {
        // Arrange
        _fixture.SeedData()
            .WithHasHadDataLockSuccess(true)
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(
                TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false,
                TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyEntityStateChangedEventPublished(Times.Never);
    }

    [Test]
    public async Task ShouldRemoveTriageDataLock_WhenNotHasHadDataLockSuccessAndNewDataLocksArePriceOnly()
    {
        // Arrange
        _fixture.SeedData()
            .WithHasHadDataLockSuccess(false)
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyDataLockTriage(40, TriageStatus.Unknown, "Should update price only datalocks when apprenticeship not HasHadDataLockSuccess");
    }

    [Test]
    public async Task ShouldPublishStateChanged_WhenNotHasHadDataLockSuccessAndNewDataLocksArePriceOnly()
    {
        // Arrange
        _fixture.SeedData()
            .WithHasHadDataLockSuccess(false)
            .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
            .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
            .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07);

        // Act
        await _fixture.Handle();

        // Assert
        _fixture.VerifyEntityStateChangedEventPublished(UserAction.RejectDataLockChange, Times.Once);
    }
}

public class RejectDataLockRequestChangesCommandHandlerTestsFixture : IDisposable
{
    public static readonly long ApprenticeshipId = 12;
    public static readonly string TrainingCourseCode100 = "100";
    public static readonly string TrainingCourseCode200 = "200";
    public static DateTime ProxyCurrentDateTime = new(2020, 1, 1);

    private const string TrainingCourseName100 = "100 Test Name";
    private readonly Fixture _autoFixture;
    private readonly RejectDataLocksRequestChangesCommand _command;
    private readonly ProviderCommitmentsDbContext _db;
    private readonly IRequestHandler<RejectDataLocksRequestChangesCommand> _handler;

    private UnitOfWorkContext UnitOfWorkContext { get; set; }

    public RejectDataLockRequestChangesCommandHandlerTestsFixture()
    {
        _autoFixture = new Fixture();
        _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _autoFixture.Customizations.Add(new ModelSpecimenBuilder());

        UnitOfWorkContext = new UnitOfWorkContext();

        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party.Employer);
            
        var currentDateTimeService = new Mock<ICurrentDateTime>();
        currentDateTimeService.Setup(x => x.UtcNow).Returns(ProxyCurrentDateTime);

        var userInfo = _autoFixture.Create<UserInfo>();
        _command = new RejectDataLocksRequestChangesCommand(ApprenticeshipId, userInfo);
            
        _handler = new RejectDataLocksRequestChangesCommandHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            Mock.Of<ILogger<RejectDataLocksRequestChangesCommandHandler>>());
    }

    public async Task Handle()
    {
        await _handler.Handle(_command, default);

        // this call is part of the DAS.SFA.UnitOfWork.Context.UnitOfWorkContext middleware in the API
        await _db.SaveChangesAsync();
    }

    public RejectDataLockRequestChangesCommandHandlerTestsFixture SeedData()
    {
        var accountLegalEntityDetails = new AccountLegalEntity()
            .Set(c => c.Id, 444);

        _db.AccountLegalEntities.Add(accountLegalEntityDetails);

        var cohortDetails = new Cohort()
            .Set(c => c.Id, 111)
            .Set(c => c.EmployerAccountId, 222)
            .Set(c => c.ProviderId, 333)
            .Set(c => c.AccountLegalEntityId, accountLegalEntityDetails.Id);

        _db.Cohorts.Add(cohortDetails);

        var priceHistoryDetails = new List<PriceHistory>()
        {
            new()
            {
                FromDate = DateTime.Now,
                ToDate = null,
                Cost = 10000,
            }
        };

        _db.PriceHistory.AddRange(priceHistoryDetails);

        var apprenticeshipDetails = _autoFixture.Build<CommitmentsV2.Models.Apprenticeship>()
            .With(s => s.Id, ApprenticeshipId)
            .With(s => s.CourseCode, TrainingCourseCode100)
            .With(s => s.CourseName, TrainingCourseName100)
            .With(s => s.ProgrammeType, ProgrammeType.Standard)
            .With(s => s.PaymentStatus, PaymentStatus.Completed)
            .With(s => s.EndDate, DateTime.UtcNow)
            .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
            .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
            .Without(s => s.Cohort)
            .Without(s => s.PriceHistory)
            .Without(s => s.ApprenticeshipUpdate)
            .Without(s => s.DataLockStatus)
            .Without(s => s.EpaOrg)
            .Without(s => s.Continuation)
            .Without(s => s.PreviousApprenticeship)
            .Create();

        // if set above in autofixture build throws exception for some obscure reason
        apprenticeshipDetails.CommitmentId = cohortDetails.Id;

        _db.Apprenticeships.Add(apprenticeshipDetails);
        _db.SaveChanges();

        return this;
    }

    public RejectDataLockRequestChangesCommandHandlerTestsFixture WithHasHadDataLockSuccess(bool hasHadDataLockSuccess)
    {
        var apprenticeship = _db.Apprenticeships.Single(p => p.Id == ApprenticeshipId);
        apprenticeship.HasHadDataLockSuccess = hasHadDataLockSuccess;
        _db.SaveChanges();
        return this;
    }

    public RejectDataLockRequestChangesCommandHandlerTestsFixture WithDataLock(long apprenticeshipId, long eventDataLockId, string ilrTrainingCourseCode, DateTime ilrEffectiveFromDate, decimal ilrTotalCost,
        bool isExpired, TriageStatus triageStatus, EventStatus eventStatus, bool isResolved, Status status, DataLockErrorCode dataLockErrorCode)
    {
        var dataLockStatus = _autoFixture
            .Build<DataLockStatus>()
            .With(p => p.ApprenticeshipId, apprenticeshipId)
            .With(p => p.DataLockEventId, eventDataLockId)
            .With(p => p.IlrTrainingCourseCode, ilrTrainingCourseCode)
            .With(p => p.IlrEffectiveFromDate, ilrEffectiveFromDate)
            .With(p => p.IlrTotalCost, ilrTotalCost)
            .With(p => p.IsExpired, isExpired)
            .With(p => p.TriageStatus, triageStatus)
            .With(p => p.EventStatus, eventStatus)
            .With(p => p.IsResolved, isResolved)
            .With(p => p.Status, status)
            .With(p => p.ErrorCode, dataLockErrorCode)
            .Without(p => p.Apprenticeship)
            .Without(p => p.ApprenticeshipUpdate)
            .Create();

        _db.DataLocks.Add(dataLockStatus);
        _db.SaveChanges();
        return this;
    }

    public void VerifyDataLockTriage(long dataLockEventId, TriageStatus triageStatus, string because)
    {
        _db.DataLocks
            .Single(p => p.DataLockEventId == dataLockEventId)
            .TriageStatus
            .Should()
            .Be(triageStatus, because);
    }

    public void VerifyEntityStateChangedEventPublished(Func<Times> times)
    {
        times().Deconstruct(out int expectedFrom, out int expectedTo);
        UnitOfWorkContext
            .GetEvents()
            .OfType<EntityStateChangedEvent>()
            .Count()
            .Should()
            .Be(expectedFrom);
    }

    public void VerifyEntityStateChangedEventPublished(UserAction userAction, Func<Times> times)
    {
        times().Deconstruct(out var expectedFrom, out _);
        UnitOfWorkContext
            .GetEvents()
            .OfType<EntityStateChangedEvent>()
            .Count(p => p.StateChangeType == userAction)
            .Should()
            .Be(expectedFrom);
    }

    public void Dispose()
    {
        _db?.Dispose();
        GC.SuppressFinalize(this);
    }
}