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
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenPausingAStartedApprenticeship : UpdateApprenticeshipStatusBase
    {
        private UpdateApprenticeshipStatusCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;
        private PaymentStatus _requestPaymentStatus = PaymentStatus.Paused;
        private PaymentStatus _apprenticeshipPaymentStatus = PaymentStatus.Active;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _exampleValidRequest = new UpdateApprenticeshipStatusCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                PaymentStatus = _requestPaymentStatus,
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
                    logger.Info($"Employer: {_exampleValidRequest.AccountId} has called UpdateApprenticeshipStatusCommand",
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

            MockApprenticeshipRespository.Verify(x => x.PauseOrResumeApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused),
                It.Is<DateTime?>(a=> a == _exampleValidRequest.DateOfChange)));
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

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(
                It.IsAny<Commitment>(), 
                It.IsAny<Apprenticeship>(), 
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused), 
                It.Is<DateTime?>(a => a.Equals(_exampleValidRequest.DateOfChange)),
                null));
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

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateNotEqualToCurrentDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            _testApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should().Be("Invalid Date of Change. Date should be todays date.");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-2).Date;
            _testApprenticeship.StartDate = startDate;

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should().Be("Invalid Date of Change. Date should be todays date.");
        }

    }
}
