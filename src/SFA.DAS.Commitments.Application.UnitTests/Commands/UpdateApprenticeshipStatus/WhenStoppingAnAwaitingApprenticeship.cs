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
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenStoppingAnAwaitingApprenticeship
    {
        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockEventsApi = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockDataLockRepository = new Mock<IDataLockRepository>();
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            _mockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            _handler = new StopApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                _mockCurrentDateTime.Object,
                _mockEventsApi.Object,
                _mockCommitmentsLogger.Object,
                _mockHistoryRepository.Object,
                _mockDataLockRepository.Object,
                _mockAcademicYearValidator.Object);

            _exampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.UtcNow.Date.AddMonths(6),
                UserName = "Bob"
            };

            _testApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            _mockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(DateTime.UtcNow);

            _mockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(_testApprenticeship);

            _mockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(
                        It.Is<long>(c => c == _testApprenticeship.CommitmentId),
                        It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                        It.Is<PaymentStatus>(s => s == PaymentStatus.Withdrawn)))
                .Returns(Task.FromResult(new object()));

            _mockDataLockRepository.Setup(x => x.GetDataLocks(_exampleValidRequest.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>());

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == _testApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
                {
                    Id = 123L,
                    EmployerAccountId = _exampleValidRequest.AccountId
                });
        }

        private StopApprenticeshipCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IApprenticeshipEvents> _mockEventsApi;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IDataLockRepository> _mockDataLockRepository;
        private StopApprenticeshipCommandHandler _handler;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        private Mock<ICommitmentsLogger> _mockCommitmentsLogger;

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }



        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(_testApprenticeship);

            await _handler.Handle(_exampleValidRequest);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(_testApprenticeship);

            _mockHistoryRepository.Verify(
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
        public async Task ThenDataLocksThatHaveBeenTriagedAsResetAreResolved()
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

            _mockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await _handler.Handle(_exampleValidRequest);

            _mockDataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved)),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogTheRequest()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentsLogger.Verify(logger =>
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

            _mockDataLockRepository.Setup(x => x.GetDataLocks(444)).ReturnsAsync(dataLocks);

            await _handler.Handle(_exampleValidRequest);

            _mockDataLockRepository.Verify(x =>
                x.UpdateDataLockStatus(It.Is<DataLockStatus>(a => a.IsResolved)), Times.Exactly(3));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.StopApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == _exampleValidRequest.DateOfChange)));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>(), It.IsAny<PaymentStatus>(), It.IsNotNull<DateTime?>(),
                It.IsAny<DateTime?>()));
        }

        [Test]
        public void ThenThrowsExceptionIfChangeDateIsNotTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            _testApprenticeship.StartDate = startDate;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }
    

        [TestCase(AcademicYearValidationResult.NotWithinFundingPeriod, false, Description =
            "Validation fails if date of change is in the previous academic year and the R14 date has passed")]
        [TestCase(AcademicYearValidationResult.Success, true, Description =
            "Validation passes if date of change is in the previous academic year and the R14 date has not passed")]
        public void ShouldThrowValidationErrorAfterR14Close(AcademicYearValidationResult academicYearValidationResult,
            bool expectedPassValidation)
        {

            _testApprenticeship.StartDate = new DateTime(2017, 3, 1); //early last academic year
            _exampleValidRequest.DateOfChange = new DateTime(2017, 5, 1); //last academic year
            _mockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2017, 10, 19, 18, 0, 1)); //after cut-off

            _mockAcademicYearValidator.Setup(x => x.Validate(It.IsAny<DateTime>()))
                .Returns(academicYearValidationResult);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

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

            _testApprenticeship.StartDate = new DateTime(2017, 3, 1);
            _exampleValidRequest.DateOfChange = new DateTime(2017, 1, 1); //last academic year
            _mockCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2017, 10, 19, 18, 0, 1)); //after cut-off

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>()
                .WithMessage("Invalid Date of Change. Date cannot be before the training start date.");
        }

    }

}