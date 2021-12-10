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

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
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

    public class RejectDataLockRequestChangesCommandHandlerTestsFixture
    {
        public static long ApprenticeshipId = 12;

        public static string TrainingCourseCode100 = "100";
        public static string TrainingCourseName100 = "100 Test Name";
        public static ProgrammeType ProgrammeType100 = ProgrammeType.Standard;

        public static string TrainingCourseCode200 = "200";
        public static string TrainingCourseName200 = "200 Test Name";
        public static ProgrammeType ProgrammeType200 = ProgrammeType.Standard;

        public static DateTime ProxyCurrentDateTime = new DateTime(2020, 1, 1);

        public Fixture AutoFixture { get; set; }

        public RejectDataLocksRequestChangesCommand Command { get; set; }

        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<RejectDataLocksRequestChangesCommand> Handler { get; set; }
        
        public UserInfo UserInfo { get; }
        public Mock<IAuthenticationService> AuthenticationService;
        public Mock<ICurrentDateTime> CurrentDateTimeService;
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup;
        
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public RejectDataLockRequestChangesCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

            UnitOfWorkContext = new UnitOfWorkContext();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party.Employer);
            
            CurrentDateTimeService = new Mock<ICurrentDateTime>();
            CurrentDateTimeService.Setup(x => x.UtcNow).Returns(ProxyCurrentDateTime);

            UserInfo = AutoFixture.Create<UserInfo>();
            Command = new RejectDataLocksRequestChangesCommand(ApprenticeshipId, UserInfo);
            
            Handler = new RejectDataLocksRequestChangesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<RejectDataLocksRequestChangesCommandHandler>>());
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, default);

            // this call is part of the DAS.SFA.UnitOfWork.Context.UnitOfWorkContext middleware in the API
            await Db.SaveChangesAsync();
        }

        public RejectDataLockRequestChangesCommandHandlerTestsFixture SeedData()
        {
            var accountLegalEntityDetails = new AccountLegalEntity()
                .Set(c => c.Id, 444);

            Db.AccountLegalEntities.Add(accountLegalEntityDetails);

            var cohortDetails = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, accountLegalEntityDetails.Id);

            Db.Cohorts.Add(cohortDetails);

            var priceHistoryDetails = new List<PriceHistory>()
            {
                new PriceHistory
                {
                    FromDate = DateTime.Now,
                    ToDate = null,
                    Cost = 10000,
                }
            };

            Db.PriceHistory.AddRange(priceHistoryDetails);

            var apprenticeshipDetails = AutoFixture.Build<CommitmentsV2.Models.Apprenticeship>()
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

            Db.Apprenticeships.Add(apprenticeshipDetails);
            Db.SaveChanges();

            return this;
        }

        public RejectDataLockRequestChangesCommandHandlerTestsFixture WithHasHadDataLockSuccess(bool hasHadDataLockSuccess)
        {
            var apprenticeship = Db.Apprenticeships.Single(p => p.Id == ApprenticeshipId);
            apprenticeship.HasHadDataLockSuccess = hasHadDataLockSuccess;
            Db.SaveChanges();
            return this;
        }

        public RejectDataLockRequestChangesCommandHandlerTestsFixture WithDataLock(long apprenticeshipId, long eventDataLockId, string ilrTrainingCourseCode, DateTime ilrEffectiveFromDate, decimal ilrTotalCost,
            bool isExpired, TriageStatus triageStatus, EventStatus eventStatus, bool isResolved, Status status, DataLockErrorCode dataLockErrorCode)
        {
            var dataLockStatus = AutoFixture
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

            Db.DataLocks.Add(dataLockStatus);
            Db.SaveChanges();
            return this;
        }

        public void VerifyDataLockTriage(long dataLockEventId, TriageStatus triageStatus, string because)
        {
            Db.DataLocks
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
            times().Deconstruct(out int expectedFrom, out int expectedTo);
            UnitOfWorkContext
                .GetEvents()
                .OfType<EntityStateChangedEvent>()
                .Where(p =>
                    p.StateChangeType == userAction)
                .Count()
                .Should()
                .Be(expectedFrom);
        }
    }
}