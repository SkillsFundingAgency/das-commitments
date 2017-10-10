using System;
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
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenStoppingAStartedApprenticeship 
    {
        private StopApprenticeshipCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;

        private PaymentStatus _requestPaymentStatus = PaymentStatus.Withdrawn;
        private PaymentStatus _apprenticeshipPaymentStatus = PaymentStatus.Active;

        private Mock<ICommitmentRepository> MockCommitmentRespository;
        private Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        private Mock<ICurrentDateTime> MockCurrentDateTime;
        private Mock<IApprenticeshipEvents> MockEventsApi;
        private Mock<IHistoryRepository> MockHistoryRepository;
        private Mock<IDataLockRepository> MockDataLockRepository;
        private StopApprenticeshipCommandHandler Handler;
        private Mock<IAcademicYearValidator> MockAcademicYearValidator;
        private Mock<ICommitmentsLogger> MockCommitmentsLogger;

       
        [SetUp]
        public void SetUp()
        {

            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();

            MockEventsApi = new Mock<IApprenticeshipEvents>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockDataLockRepository = new Mock<IDataLockRepository>();

            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            MockAcademicYearValidator = new Mock<IAcademicYearValidator>();


            MockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            Handler = new StopApprenticeshipCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                MockCurrentDateTime.Object,
                MockEventsApi.Object,
                MockCommitmentsLogger.Object,
                MockHistoryRepository.Object,
                MockDataLockRepository.Object,
                MockAcademicYearValidator.Object);
        
            _exampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.Now.Date,
                UserName = "Bob"
            };

            _testApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = _apprenticeshipPaymentStatus,
                StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            };
            MockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(_testApprenticeship);
            MockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>())).Returns(Task.FromResult(new object()));
            MockDataLockRepository.Setup(x => x.GetDataLocks(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(new List<DataLockStatus>());


        }

        [Test]
        public async Task ItShouldLogTheRequest()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await Handler.Handle(_exampleValidRequest);

            MockCommitmentsLogger.Verify(logger =>
                    logger.Info($"Employer: {_exampleValidRequest.AccountId} has called StopApprenticeshipCommand",
                        _exampleValidRequest.AccountId,
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        _exampleValidRequest.ApprenticeshipId,
                        It.IsAny<int?>(),
                        _exampleValidRequest.Caller)
                , Times.Once);
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await Handler.Handle(_exampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.StopApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == _exampleValidRequest.DateOfChange)));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await Handler.Handle(_exampleValidRequest);

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<PaymentStatus>(), It.IsNotNull<DateTime?>(), It.IsAny<DateTime?>()));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                ProviderId = _exampleValidRequest.AccountId++
            });

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateIsNotTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            _testApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            _testApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsBeforeTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            _testApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = startDate.AddDays(-5).Date;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(_testApprenticeship);

            await Handler.Handle(_exampleValidRequest);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(_testApprenticeship);

            MockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == _testApprenticeship.Id &&
                                y.First().ChangeType == ApprenticeshipChangeType.ChangeOfStatus.ToString() &&
                                y.First().EntityType == "Apprenticeship" &&
                                y.First().OriginalState == expectedOriginalApprenticeshipState &&
                                y.First().UpdatedByRole == CallerType.Employer.ToString() &&
                                y.First().UpdatedState == expectedNewApprenticeshipState &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);
        }

        [Test]
        public async Task ThenACourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock04 }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(_exampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved == true)), Times.Once);
        }

        [Test]
        public async Task ThenMultipleCourseDataLocksThatHaveBeenTriagedAsResetAreResolved()
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock04 },
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock03 },
                new DataLockStatus { TriageStatus = TriageStatus.Unknown, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock07 }, // Is a price error
                new DataLockStatus { TriageStatus = TriageStatus.Restart, IsResolved = false, ErrorCode = DataLockErrorCode.Dlock06 }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await Handler.Handle(_exampleValidRequest);

            MockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved == true)), Times.Exactly(3));
        }

        [TestCase(AcademicYearValidationResult.NotWithinFundingPeriod, false, Description = "Validation fails if date of change is in the previous academic year and the R14 date has passed")]
        [TestCase(AcademicYearValidationResult.Success, true, Description = "Validation passes if date of change is in the previous academic year and the R14 date has not passed")]
        public void ShouldThrowValidationErrorAfterR14Close(AcademicYearValidationResult academicYearValidationResult, bool expectedPassValidation)
        {
            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _testApprenticeship.StartDate = new DateTime(2016, 3, 1); //early last academic year
            _exampleValidRequest.DateOfChange = new DateTime(2016, 5, 1); //last academic year
            MockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2016, 10, 19)); //after cut-off

            MockAcademicYearValidator.Setup(x => x.Validate(It.IsAny<DateTime>())).Returns(academicYearValidationResult);

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

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
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _testApprenticeship.StartDate = new DateTime(2016, 3, 1);
            _exampleValidRequest.DateOfChange = new DateTime(2016, 1, 1); //last academic year
            MockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2016, 10, 19)); //after cut-off

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>()
                .WithMessage("Invalid Date of Change. Date cannot be before the training start date.");
        }
    }
}
