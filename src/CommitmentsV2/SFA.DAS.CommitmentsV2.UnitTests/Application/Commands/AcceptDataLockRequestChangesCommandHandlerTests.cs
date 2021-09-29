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
    using TestsFixture = AcceptDataLockRequestChangesCommandHandlerTestsFixture;

    [TestFixture]
    [Parallelizable]
    public class AcceptDataLockRequestChangesCommandHandlerTests
    {
        private TestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new TestsFixture();
        }

        [Test]
        public async Task ShouldNotResolveDataLock_WhenNoNewDataLockToProcess()
        {
            // Arrange
            _fixture.SeedData()
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId, 50, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddMonths(1), 2000, false, TriageStatus.Change, EventStatus.New, false, Status.Pass, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 60, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime.AddMonths(2), 3000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockResolved(40, true, "Should not update already resolved datalocks");
            _fixture.VerifyDataLockResolved(50, false, "Should not update passed datalocks");
            _fixture.VerifyDataLockResolved(60, false, "Should not update datalocks with triage status 'unknown' even when unhandled");
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
        public async Task ShouldNotPublishDataLockTriage_WhenNoNewDataLockToProcess()
        {
            // Arrange
            _fixture.SeedData()
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId, 50, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddMonths(1), 2000, false, TriageStatus.Change, EventStatus.New, false, Status.Pass, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 60, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime.AddMonths(2), 3000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockTriageApprovedEventPublished(Times.Never);
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
            _fixture.VerifyDataLockResolved(40, false, "Should not update price only datalocks when apprenticeship HasHadDataLockSuccess");
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
        public async Task ShouldNotPublishDataLockTriage_WhenHasHadDataLockSuccessAndNewDataLocksAreNotPriceOnly()
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
            _fixture.VerifyDataLockTriageApprovedEventPublished(Times.Never);
        }

        [Test]
        public async Task ShouldResolveDataLock_WhenNotHasHadDataLockSuccessAndNewDataLocksArePriceOnly()
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
            _fixture.VerifyDataLockResolved(40, true, "Should update price only datalocks when apprenticeship not HasHadDataLockSuccess");
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
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdatePriceHistory, Times.Once);
        }

        [Test]
        public async Task ShouldPublishDataLockTriage_WhenNotHasHadDataLockSuccessAndNewDataLocksArePriceOnly()
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
            _fixture.VerifyDataLockTriageApprovedEventPublished(TestsFixture.ApprenticeshipId, TestsFixture.ProxyCurrentDateTime,
                new PriceEpisode[]
                {
                    new PriceEpisode { FromDate = TestsFixture.ProxyCurrentDateTime, ToDate = null, Cost = 1000 }
                }, TestsFixture.TrainingCourseCode100, TestsFixture.ProgrammeType100, Times.Once);
        }

        [TestCaseSource(typeof(ShouldUpdatePriceHistoryDataCases))]
        public async Task ShouldResolveDataLocks(ShouldUpdatePriceHistoryDataCases.Setup setup, ShouldUpdatePriceHistoryDataCases.InputDataLock[] inputDataLocks,
            ShouldUpdatePriceHistoryDataCases.ExpectedOutput expectedOutput)
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(setup.HasHadDataLockSuccess)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07);

            inputDataLocks.ToList().ForEach(p =>
                _fixture.WithDataLock(p.ApprenticeshipId, p.EventDataLockId, p.IlrTrainingCourseCode, p.IlrEffectiveFromDate, p.IlrTotalCost, p.IsExpired,
                    p.TriageStatus, p.EventStatus, p.IsResolved, p.Status, p.DataLockErrorCode));

            // Act
            await _fixture.Handle();

            // Assert
            expectedOutput.OutputResolvedEventDataLockIds.ToList().ForEach(p =>
                _fixture.VerifyDataLockResolved(p.EventDataLockId, p.IsResolved, p.Because));
        }

        [TestCaseSource(typeof(ShouldUpdatePriceHistoryDataCases))]
        public async Task ShouldPublishStateChangedEvent(ShouldUpdatePriceHistoryDataCases.Setup setup, ShouldUpdatePriceHistoryDataCases.InputDataLock[] inputDataLocks,
            ShouldUpdatePriceHistoryDataCases.ExpectedOutput expectedOutput)
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(setup.HasHadDataLockSuccess)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07);

            inputDataLocks.ToList().ForEach(p =>
                _fixture.WithDataLock(p.ApprenticeshipId, p.EventDataLockId, p.IlrTrainingCourseCode, p.IlrEffectiveFromDate, p.IlrTotalCost, p.IsExpired,
                    p.TriageStatus, p.EventStatus, p.IsResolved, p.Status, p.DataLockErrorCode));

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdatePriceHistory, () => Times.Exactly(expectedOutput.OutputPriceHistories.Length));
        }

        [TestCaseSource(typeof(ShouldUpdatePriceHistoryDataCases))]
        public async Task ShouldPublishDataLockTriage(ShouldUpdatePriceHistoryDataCases.Setup setup, ShouldUpdatePriceHistoryDataCases.InputDataLock[] inputDataLocks,
            ShouldUpdatePriceHistoryDataCases.ExpectedOutput expectedOutput)
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(setup.HasHadDataLockSuccess)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07);


            inputDataLocks.ToList().ForEach(p =>
                _fixture.WithDataLock(p.ApprenticeshipId, p.EventDataLockId, p.IlrTrainingCourseCode, p.IlrEffectiveFromDate, p.IlrTotalCost, p.IsExpired,
                    p.TriageStatus, p.EventStatus, p.IsResolved, p.Status, p.DataLockErrorCode));

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockTriageApprovedEventPublished(TestsFixture.ApprenticeshipId, TestsFixture.ProxyCurrentDateTime,
                expectedOutput.OutputPriceHistories.ToList().Select(p => new PriceEpisode { FromDate = p.FromDate, ToDate = p.ToDate, Cost = p.Cost }).ToArray(),
                expectedOutput.CourseCode, expectedOutput.ProgrammeType, Times.Once);
        }

        [Test]
        public async Task ShouldUpdateCourse_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(false)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.ApprenticeshipFromDb.CourseCode.Should().Be(TestsFixture.TrainingCourseCode200, "Course code should update for course data lock when not has had datalock success");
            _fixture.ApprenticeshipFromDb.CourseName.Should().Be(TestsFixture.TrainingCourseName200, "Course name should update for course data lock when not has had datalock success");
        }

        [Test]
        public async Task ShouldResolveDataLocks_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(false)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockResolved(41, true, "Course data lock should be resolved");
        }

        [Test]
        public async Task ShouldPublishStateChanged_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(false)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdatePriceHistory, ()=> Times.Exactly(2));
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdateCourse, Times.Once);
        }

        [Test]
        public async Task ShouldPublishStateChanged_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentPrice()
        {
            // Arrange
            _fixture.SeedData(true)
                .WithHasHadDataLockSuccess(false)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1500, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 2500, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.DeletePriceHistory, Times.Once);
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdatePriceHistory, () => Times.Exactly(2));
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdateCourse, Times.Once);
        }
        
        [Test]
        public async Task ShouldPublishDataLockTriage_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(false)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockTriageApprovedEventPublished(TestsFixture.ApprenticeshipId, TestsFixture.ProxyCurrentDateTime,
                new PriceEpisode[]
                {
                    new PriceEpisode { FromDate = TestsFixture.ProxyCurrentDateTime, ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(19), Cost = 1000 },
                    new PriceEpisode { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(20), ToDate = null, Cost = 1000 }
                }, TestsFixture.TrainingCourseCode200, TestsFixture.ProgrammeType200, Times.Once);
        }

        [Test]
        public async Task ShouldNotUpdateCourse_WhenHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(true)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.ApprenticeshipFromDb.CourseCode.Should().Be(TestsFixture.TrainingCourseCode100, "Course code should not update for course data lock when has had datalock success");
            _fixture.ApprenticeshipFromDb.CourseName.Should().Be(TestsFixture.TrainingCourseName100, "Course name should not update for course data lock when has had datalock success");
        }

        [Test]
        public async Task ShouldNotResolveDataLocks_WhenHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(true)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLockResolved(41, false, "Course data lock should not be resolved when had had datalock success");
        }

        [Test]
        public async Task ShouldNotPublishStateChanged_WhenNotHasHadDataLockSuccessAndNewDataLocksHasDifferentCourse()
        {
            // Arrange
            _fixture.SeedData()
                .WithHasHadDataLockSuccess(true)
                .WithDataLock(TestsFixture.ApprenticeshipId + 1, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Unknown, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId + 2, 20, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock03)
                .WithDataLock(TestsFixture.ApprenticeshipId + 3, 30, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Unknown, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 40, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 41, TestsFixture.TrainingCourseCode200, TestsFixture.ProxyCurrentDateTime.AddDays(20), 1000, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock03);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdatePriceHistory, Times.Never);
            _fixture.VerifyEntityStateChangedEventPublished(UserAction.UpdateCourse, Times.Never);
        }

        [Test]
        public async Task ShouldNotDuplicatePriceHistory_WhenMultipleDataLockStatusForSameCostAndFromDate()
        {
            // Arrange
            _fixture.SeedData(withPriceHistory: false)
                .WithHasHadDataLockSuccess(true)
                .WithDataLock(TestsFixture.ApprenticeshipId, 10, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime.AddDays(-2), 1000, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 11, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1100, false, TriageStatus.Change, EventStatus.New, true, Status.Fail, DataLockErrorCode.Dlock07)
                .WithDataLock(TestsFixture.ApprenticeshipId, 12, TestsFixture.TrainingCourseCode100, TestsFixture.ProxyCurrentDateTime, 1100, false, TriageStatus.Change, EventStatus.New, false, Status.Fail, DataLockErrorCode.Dlock07);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyNoDuplicatePriceHistory(TestsFixture.ApprenticeshipId, 2);
        }

        public class ShouldUpdatePriceHistoryDataCases : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                // define some datalocks which are pre-existing in all cases
                var previouslyResolvedPriceOnlyDataLock40 = new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 40, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(10), IlrTotalCost = 1000, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = true, Status = Status.Fail, DataLockErrorCode = DataLockErrorCode.Dlock07 };
                var previouslyResolvedPriceOnlyDataLock41 = new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 41, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(20), IlrTotalCost = 1100, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = true, Status = Status.Fail, DataLockErrorCode = DataLockErrorCode.Dlock07 };
                var passDataLock50 = new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 50, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(30), IlrTotalCost = 1200, IsExpired = false, TriageStatus = TriageStatus.Unknown, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Pass, DataLockErrorCode = DataLockErrorCode.Dlock07 };
                var passDataLock51 = new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 51, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(40), IlrTotalCost = 1300, IsExpired = false, TriageStatus = TriageStatus.Unknown, EventStatus = EventStatus.New, IsResolved = true, Status = Status.Pass, DataLockErrorCode = DataLockErrorCode.Dlock07 };
                var passDataLock52 = new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 52, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(50), IlrTotalCost = 1400, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Pass, DataLockErrorCode = DataLockErrorCode.Dlock07 };

                var previouslyResolvedPriceOnlyDataLock40PriceHistory = new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(10), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(19), Cost = 1000 };
                var previouslyResolvedPriceOnlyDataLock41PriceHistory = new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(20), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(29), Cost = 1100 };
                var passDataLock50PriceHistory = new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(30), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(39), Cost = 1200 };
                var passDataLock51PriceHistory = new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(40), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(49), Cost = 1300 };
                var passDataLock52PriceHistory = new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(50), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(59), Cost = 1400 };

                // has had datalock success will not include course datalocks in price history
                yield return new object[]
                {
                    new Setup
                    {
                        ApprenticeshipId = TestsFixture.ApprenticeshipId,
                        HasHadDataLockSuccess = true
                    },
                    new InputDataLock[]
                    {
                        previouslyResolvedPriceOnlyDataLock40, previouslyResolvedPriceOnlyDataLock41,
                        passDataLock50, passDataLock51, passDataLock52,
                        // price datalock 60 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 60, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), IlrTotalCost = 1500, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                        // course datalock 61 will not be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 61, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode200, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(70), IlrTotalCost = 1500, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock03 },
                        // price datalock 62 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 62, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), IlrTotalCost = 1700, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                    },
                    new ExpectedOutput
                    {
                        OutputResolvedEventDataLockIds = new OutputResolvedEventDataLockId[]
                        {
                            new OutputResolvedEventDataLockId { EventDataLockId = 60, IsResolved = true, Because = "New price data lock should always be resolved"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 61, IsResolved = false, Because = "New course data lock should not be resolved when HadHadDataLockSuccess is true"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 62, IsResolved = true, Because = "New price data lock should always be resolved"}
                        },
                        OutputPriceHistories =  new OutputPriceHistory[]
                        {
                            previouslyResolvedPriceOnlyDataLock40PriceHistory, previouslyResolvedPriceOnlyDataLock41PriceHistory,
                            passDataLock50PriceHistory, passDataLock51PriceHistory, passDataLock52PriceHistory,
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(79), Cost = 1500 },
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), ToDate = null, Cost = 1700 }
                        },
                        CourseCode = TestsFixture.TrainingCourseCode100,
                        ProgrammeType = TestsFixture.ProgrammeType100
                    }
                };

                // has had datalock success will not include course/price datalocks in price history
                yield return new object[]
                {
                    new Setup
                    {
                        ApprenticeshipId = TestsFixture.ApprenticeshipId,
                        HasHadDataLockSuccess = true
                    },
                    new InputDataLock[]
                    {
                        previouslyResolvedPriceOnlyDataLock40, previouslyResolvedPriceOnlyDataLock41,
                        passDataLock50, passDataLock51, passDataLock52,
                        // price datalock 60 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 60, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), IlrTotalCost = 1500, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                        // course/price datalock 61 will not be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 61, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode200, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(70), IlrTotalCost = 1600, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock07 },
                        // price datalock 62 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 62, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), IlrTotalCost = 1700, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                    },
                    new ExpectedOutput
                    {
                        OutputResolvedEventDataLockIds = new OutputResolvedEventDataLockId[]
                        {
                            new OutputResolvedEventDataLockId { EventDataLockId = 60, IsResolved = true, Because = "New price data lock should always be resolved"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 61, IsResolved = false, Because = "New course/price data lock should not be resolved when HadHadDataLockSuccess is true"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 62, IsResolved = true, Because = "New price data lock should always be resolved"}
                        },
                        OutputPriceHistories = new OutputPriceHistory[]
                        {
                            previouslyResolvedPriceOnlyDataLock40PriceHistory, previouslyResolvedPriceOnlyDataLock41PriceHistory,
                            passDataLock50PriceHistory, passDataLock51PriceHistory, passDataLock52PriceHistory,
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(79), Cost = 1500 },
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), ToDate = null, Cost = 1700 }
                        },
                        CourseCode = TestsFixture.TrainingCourseCode100,
                        ProgrammeType = TestsFixture.ProgrammeType100
                    }
                };

                // not has had datalock success will include course/price datalocks
                yield return new object[]
                {
                    new Setup
                    {
                        ApprenticeshipId = TestsFixture.ApprenticeshipId,
                        HasHadDataLockSuccess = false
                    },
                    new InputDataLock[]
                    {
                        previouslyResolvedPriceOnlyDataLock40, previouslyResolvedPriceOnlyDataLock41,
                        passDataLock50, passDataLock51, passDataLock52,
                        // price datalock 60 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 60, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), IlrTotalCost = 1500, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                        // course/price datalock 61 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 61, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode200, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(70), IlrTotalCost = 1600, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock07 },
                        // price datalock 62 will be included in the price history
                        new InputDataLock { ApprenticeshipId = TestsFixture.ApprenticeshipId, EventDataLockId = 62, IlrTrainingCourseCode = TestsFixture.TrainingCourseCode100, IlrEffectiveFromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), IlrTotalCost = 1700, IsExpired = false, TriageStatus = TriageStatus.Change, EventStatus = EventStatus.New, IsResolved = false, Status = Status.Unknown, DataLockErrorCode = DataLockErrorCode.Dlock07 },
                    },
                    new ExpectedOutput
                    {
                        OutputResolvedEventDataLockIds = new OutputResolvedEventDataLockId[]
                        {
                            new OutputResolvedEventDataLockId { EventDataLockId = 60, IsResolved = true, Because = "New price data lock should always be resolved"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 61, IsResolved = true, Because = "New course/price data lock should be resolved when HadHadDataLockSuccess is false"},
                            new OutputResolvedEventDataLockId { EventDataLockId = 62, IsResolved = true, Because = "New price data lock should always be resolved"}
                        },
                        OutputPriceHistories = new OutputPriceHistory[]
                        {
                            previouslyResolvedPriceOnlyDataLock40PriceHistory, previouslyResolvedPriceOnlyDataLock41PriceHistory,
                            passDataLock50PriceHistory, passDataLock51PriceHistory, passDataLock52PriceHistory,
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(60), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(69), Cost = 1500 },
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(70), ToDate = TestsFixture.ProxyCurrentDateTime.AddDays(79), Cost = 1600 },
                            new OutputPriceHistory { FromDate = TestsFixture.ProxyCurrentDateTime.AddDays(80), ToDate = null, Cost = 1700 }
                        },
                        CourseCode = TestsFixture.TrainingCourseCode200,
                        ProgrammeType = TestsFixture.ProgrammeType200
                    }
                };
            }

            public class Setup
            {
                public long ApprenticeshipId { get; set; }
                public bool HasHadDataLockSuccess { get; set; }
            }

            public class InputDataLock
            {
                public long ApprenticeshipId { get; set; }
                public long EventDataLockId { get; set; }
                public string IlrTrainingCourseCode { get; set; }
                public DateTime IlrEffectiveFromDate { get; set; }
                public decimal IlrTotalCost { get; set; }
                public bool IsExpired { get; set; }
                public TriageStatus TriageStatus { get; set; }
                public EventStatus EventStatus { get; set; }
                public bool IsResolved { get; set; }
                public Status Status { get; set; }
                public DataLockErrorCode DataLockErrorCode { get; set; }
            }

            public class ExpectedOutput
            {
                public OutputResolvedEventDataLockId[] OutputResolvedEventDataLockIds { get; set; }
                public OutputPriceHistory[] OutputPriceHistories { get; set; }
                public string CourseCode { get; set; }
                public ProgrammeType ProgrammeType { get; set; }
            }

            public class OutputResolvedEventDataLockId
            {
                public long EventDataLockId { get; set; }
                public bool IsResolved { get; set; }
                public string Because { get; set; }
            }

            public class OutputPriceHistory
            {
                public DateTime FromDate { get; set; }
                public DateTime? ToDate { get; set; }
                public decimal Cost { get; set; }
            }
        }
    }

    public class AcceptDataLockRequestChangesCommandHandlerTestsFixture
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

        public AcceptDataLocksRequestChangesCommand Command { get; set; }

        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<AcceptDataLocksRequestChangesCommand> Handler { get; set; }

        public UserInfo UserInfo { get; }
        public Mock<IAuthenticationService> AuthenticationService;
        public Mock<ICurrentDateTime> CurrentDateTimeService;
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup;

        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public Apprenticeship ApprenticeshipFromDb =>
            Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
        public PriceHistory PriceHistoryFromDb =>
          Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).PriceHistory.First();

        public AcceptDataLockRequestChangesCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

            UnitOfWorkContext = new UnitOfWorkContext();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party.Employer);

            CurrentDateTimeService = new Mock<ICurrentDateTime>();
            CurrentDateTimeService.Setup(x => x.UtcNow).Returns(ProxyCurrentDateTime);

            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            TrainingProgrammeLookup.Setup(x => x.GetTrainingProgramme(TrainingCourseCode100))
                .ReturnsAsync(new CommitmentsV2.Domain.Entities.TrainingProgramme(TrainingCourseCode100, TrainingCourseName100, ProgrammeType.Standard, DateTime.Now, DateTime.Now));
            TrainingProgrammeLookup.Setup(x => x.GetTrainingProgramme(TrainingCourseCode200))
                .ReturnsAsync(new CommitmentsV2.Domain.Entities.TrainingProgramme(TrainingCourseCode200, TrainingCourseName200, ProgrammeType.Standard, DateTime.Now, DateTime.Now));

            UserInfo = AutoFixture.Create<UserInfo>();
            Command = new AcceptDataLocksRequestChangesCommand(ApprenticeshipId, UserInfo);

            Handler = new AcceptDataLocksRequestChangesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                CurrentDateTimeService.Object,
                TrainingProgrammeLookup.Object,
                Mock.Of<ILogger<AcceptDataLocksRequestChangesCommandHandler>>());
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, default);

            // this call is part of the DAS.SFA.UnitOfWork.Context.UnitOfWorkContext middleware in the API
            await Db.SaveChangesAsync();
        }

        public AcceptDataLockRequestChangesCommandHandlerTestsFixture SeedData(bool withPriceHistory = true)
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

            if (withPriceHistory)
            {
                var priceHistoryDetails = new List<PriceHistory>()
                {
                    new PriceHistory
                    {
                        FromDate = DateTime.Now,
                        ToDate = null,
                        Cost = 10000,
                        ApprenticeshipId = apprenticeshipDetails.Id
                    }
                };

                Db.PriceHistory.AddRange(priceHistoryDetails);
            }
            
            Db.SaveChanges();

            return this;
        }

        public AcceptDataLockRequestChangesCommandHandlerTestsFixture WithHasHadDataLockSuccess(bool hasHadDataLockSuccess)
        {
            var apprenticeship = Db.Apprenticeships.Single(p => p.Id == ApprenticeshipId);
            apprenticeship.HasHadDataLockSuccess = hasHadDataLockSuccess;
            Db.SaveChanges();
            return this;
        }

        public AcceptDataLockRequestChangesCommandHandlerTestsFixture WithDataLock(long apprenticeshipId, long eventDataLockId, string ilrTrainingCourseCode, DateTime ilrEffectiveFromDate, decimal ilrTotalCost,
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

        public void VerifyNoDuplicatePriceHistory(long apprenticeshipId, int count)
        {
            Db.PriceHistory.Count().Should().Be(count);

            Db.PriceHistory
                .Where(ph => ph.ApprenticeshipId == apprenticeshipId)
                .GroupBy(ph => new { ph.Cost, ph.FromDate })
                .Any(grp => grp.Count() > 1)
                .Should()
                .BeFalse();
        }

        public void VerifyDataLockResolved(long dataLockEventId, bool isResolved, string because)
        {
            Db.DataLocks
                .Single(p => p.DataLockEventId == dataLockEventId)
                .IsResolved
                .Should()
                .Be(isResolved, because);
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

        public void VerifyDataLockTriageApprovedEventPublished(Func<Times> times)
        {
            times().Deconstruct(out int expectedFrom, out int expectedTo);
            UnitOfWorkContext
                .GetEvents()
                .OfType<DataLockTriageApprovedEvent>()
                .Count()
                .Should()
                .Be(expectedFrom);
        }

        public void VerifyDataLockTriageApprovedEventPublished(long apprenticeshipId, DateTime approvedOn, PriceEpisode[] priceEpisodes,
            string trainingCode, ProgrammeType trainingType, Func<Times> times)
        {
            times().Deconstruct(out int expectedFrom, out int expectedTo);
            var events = UnitOfWorkContext
                .GetEvents()
                .OfType<DataLockTriageApprovedEvent>();

            events
                .Count()
                .Should()
                .Be(expectedFrom);

            events.ToList().ForEach(p =>
            {
                p.Should().BeEquivalentTo(new DataLockTriageApprovedEvent()
                {
                    ApprenticeshipId = apprenticeshipId,
                    ApprovedOn = approvedOn,
                    PriceEpisodes = priceEpisodes,
                    TrainingCode = trainingCode,
                    TrainingType = trainingType
                });
            });
        }
    }
}