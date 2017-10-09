using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenStoppingAStartedApprenticeship : UpdateApprenticeshipStatusBase
    {
        protected override PaymentStatus RequestPaymentStatus => PaymentStatus.Withdrawn;
        protected override PaymentStatus ApprenticeshipPaymentStatus => PaymentStatus.Active;

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.StopApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == ExampleValidRequest.DateOfChange)));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            await Handler.Handle(ExampleValidRequest);

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<PaymentStatus>(), It.IsNotNull<DateTime?>(), It.IsAny<DateTime?>()));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            ExampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                ProviderId = ExampleValidRequest.AccountId++
            });

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<Exception>();
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateIsNotTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            TestApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            ExampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsBeforeTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            ExampleValidRequest.DateOfChange = startDate.AddDays(-5).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

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
        public async Task ThenACourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock04 }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved == true)), Times.Once);
        }

        [Test]
        public async Task ThenMultipleCourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock04 },
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock03 },
                new DataLockStatus { TriageStatus = TriageStatus.Unknown, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock07 }, // Is a price error
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock06 }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved == true)), Times.Exactly(3));
        }

        [TestCase(AcademicYearValidationResult.NotWithinFundingPeriod, false, Description = "Validation fails if date of change is in the previous academic year and the R14 date has passed")]
        [TestCase(AcademicYearValidationResult.Success, true, Description = "Validation passes if date of change is in the previous academic year and the R14 date has not passed")]
        public void ShouldThrowValidationErrorAfterR14Close(AcademicYearValidationResult academicYearValidationResult, bool expectedPassValidation)
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            TestApprenticeship.StartDate = new DateTime(2016, 3, 1); //early last academic year
            ExampleValidRequest.DateOfChange = new DateTime(2016, 5, 1); //last academic year
            MockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2016, 10, 19)); //after cut-off

            MockAcademicYearValidator.Setup(x => x.Validate(It.IsAny<DateTime>())).Returns(academicYearValidationResult);

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            if (expectedPassValidation)
            {
                act.ShouldNotThrow<ValidationException>();
            }
            else
            {
                act.ShouldThrow<ValidationException>().WithMessage("Invalid Date of Change. Date cannot be before the academic year start date.");
            }
        }

        [Test(Description = "Validation fails for both R14 having passed and change date before Start Date - Start Date error takes precedence")]
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
    }
}
