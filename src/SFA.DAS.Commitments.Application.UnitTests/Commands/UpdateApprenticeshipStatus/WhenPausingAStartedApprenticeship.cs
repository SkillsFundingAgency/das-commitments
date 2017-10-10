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
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenPausingAStartedApprenticeship
    {
        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();

            _mockEventsApi = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();

            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);


            _mockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            _handler = new PauseApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                _mockCurrentDateTime.Object,
                _mockEventsApi.Object,
                _mockCommitmentsLogger.Object,
                _mockHistoryRepository.Object);

            _exampleValidRequest = new PauseApprenticeshipCommand
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
            _mockApprenticeshipRespository
                .Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(_testApprenticeship);
            _mockApprenticeshipRespository
                .Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>()))
                .Returns(Task.FromResult(new object()));
        }

        private PauseApprenticeshipCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;
        private PaymentStatus _requestPaymentStatus = PaymentStatus.Paused;
        private readonly PaymentStatus _apprenticeshipPaymentStatus = PaymentStatus.Active;
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IApprenticeshipEvents> _mockEventsApi;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private PauseApprenticeshipCommandHandler _handler;
        private Mock<ICommitmentsLogger> _mockCommitmentsLogger;

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }

        [Test]
        public async Task ItShouldLogTheRequest()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentsLogger.Verify(logger =>
                    logger.Info($"Employer: {_exampleValidRequest.AccountId} has called PauseApprenticeshipCommand",
                        _exampleValidRequest.AccountId,
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        _exampleValidRequest.ApprenticeshipId,
                        It.IsAny<int?>(),
                        _exampleValidRequest.Caller)
                , Times.Once);
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

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
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.PauseOrResumeApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused),
                It.Is<DateTime?>(a => a == _exampleValidRequest.DateOfChange)));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(
                It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>(),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused),
                It.Is<DateTime?>(a => a.Equals(_exampleValidRequest.DateOfChange)),
                null));
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-2).Date;
            _testApprenticeship.StartDate = startDate;

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should()
                .Be("Invalid Date of Change. Date should be todays date.");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateNotEqualToCurrentDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            _testApprenticeship.StartDate = startDate;

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should()
                .Be("Invalid Date of Change. Date should be todays date.");
        }
    }
}