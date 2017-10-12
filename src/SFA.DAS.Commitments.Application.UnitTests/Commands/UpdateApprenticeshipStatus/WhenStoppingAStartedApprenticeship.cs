﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenStoppingAStartedApprenticeship : WhenStoppingAnApprenticeship
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();


            ExampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.Now.Date,
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            };


            MockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(DateTime.UtcNow);


            MockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);
            MockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>()))
                .Returns(Task.FromResult(new object()));
            MockDataLockRepository.Setup(x => x.GetDataLocks(ExampleValidRequest.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>());

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }


        [TestCase(AcademicYearValidationResult.NotWithinFundingPeriod, false, Description =
            "Validation fails if date of change is in the previous academic year and the R14 date has passed")]
        [TestCase(AcademicYearValidationResult.Success, true, Description =
            "Validation passes if date of change is in the previous academic year and the R14 date has not passed")]
        public void ShouldThrowValidationErrorAfterR14Close(AcademicYearValidationResult academicYearValidationResult,
            bool expectedPassValidation)
        {
            TestApprenticeship.StartDate = new DateTime(2016, 3, 1); //early last academic year
            ExampleValidRequest.DateOfChange = new DateTime(2016, 5, 1); //last academic year
            MockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2016, 10, 19)); //after cut-off

            MockAcademicYearValidator.Setup(x => x.Validate(It.IsAny<DateTime>()))
                .Returns(academicYearValidationResult);

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            if (expectedPassValidation)
                act.ShouldNotThrow<ValidationException>();
            else
                act.ShouldThrow<ValidationException>()
                    .WithMessage("Invalid Date of Change. Date cannot be before the academic year start date.");
        }

        [Test(Description =
            "Validation fails for both R14 having passed and change date before Start Date - Start Date error takes precedence")]
        public void ShouldThrowStartDateValidationErrorAfterR14CloseAndStopDateBeforeStartDate()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            TestApprenticeship.StartDate = new DateTime(2016, 3, 1);
            ExampleValidRequest.DateOfChange = new DateTime(2016, 1, 1); //last academic year
            MockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2016, 10, 19)); //after cut-off

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>()
                .WithMessage("Invalid Date of Change. Date cannot be before the training start date.");
        }

        [Test]
        public async Task ThenACourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    TriageStatus = TriageStatus.Restart,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock04
                }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved)),
                Times.Once);
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(TestApprenticeship);

            await Handler.Handle(ExampleValidRequest);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(TestApprenticeship);

            MockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == TestApprenticeship.Id &&
                                y.First().ChangeType == ApprenticeshipChangeType.ChangeOfStatus.ToString() &&
                                y.First().EntityType == "Apprenticeship" &&
                                y.First().OriginalState == expectedOriginalApprenticeshipState &&
                                y.First().UpdatedByRole == CallerType.Employer.ToString() &&
                                y.First().UpdatedState == expectedNewApprenticeshipState &&
                                y.First().UserId == ExampleValidRequest.UserId &&
                                y.First().UpdatedByName == ExampleValidRequest.UserName)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogTheRequest()
        {
            await Handler.Handle(ExampleValidRequest);

            MockCommitmentsLogger.Verify(logger =>
                    logger.Info($"Employer: {ExampleValidRequest.AccountId} has called StopApprenticeshipCommand",
                        ExampleValidRequest.AccountId,
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        ExampleValidRequest.ApprenticeshipId,
                        It.IsAny<int?>(),
                        ExampleValidRequest.Caller)
                , Times.Once);
        }

        [Test]
        public async Task ThenMultipleCourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    TriageStatus = TriageStatus.Restart,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock04
                },
                new DataLockStatus
                {
                    TriageStatus = TriageStatus.Restart,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock03
                },
                new DataLockStatus
                {
                    TriageStatus = TriageStatus.Unknown,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock07
                }, // Is a price error
                new DataLockStatus
                {
                    TriageStatus = TriageStatus.Restart,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock06
                }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved)),
                Times.Exactly(3));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.StopApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == ExampleValidRequest.DateOfChange)));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            await Handler.Handle(ExampleValidRequest);

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>(), It.IsAny<PaymentStatus>(), It.IsNotNull<DateTime?>(),
                It.IsAny<DateTime?>()));
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsBeforeTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;


            ExampleValidRequest.DateOfChange = startDate.AddDays(-5).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;

            ExampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateIsNotTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            TestApprenticeship.StartDate = startDate;


            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }


        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            ExampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }
    }
}