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
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenStoppingAnAwaitingApprenticeship : WhenStoppingAnApprenticeship
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();


            ExampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.UtcNow.Date.AddMonths(6),
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                Id = 444L,
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            MockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(DateTime.UtcNow);

            MockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(
                        It.Is<long>(c => c == TestApprenticeship.CommitmentId),
                        It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                        It.Is<PaymentStatus>(s => s == PaymentStatus.Withdrawn)))
                .Returns(Task.FromResult(new object()));

            MockDataLockRepository.Setup(x => x.GetDataLocks(ExampleValidRequest.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>());

            MockDataLockRepository.Setup(x => x.ResolveDataLock(It.IsAny<IEnumerable<long>>()))
                .Returns(Task.CompletedTask);

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == TestApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
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
                                y.First().ChangeType == ApprenticeshipChangeType.ChangeOfStatus.ToString() &&
                                y.First().CommitmentId == null &&
                                y.First().ApprenticeshipId == TestApprenticeship.Id &&
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
        public async Task ThenAllOutstandingDataLocksAreResolved()
        {
            var dataLocks = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 1,
                    TriageStatus = TriageStatus.Restart,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock04
                },
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    TriageStatus = TriageStatus.Unknown,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock03
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    TriageStatus = TriageStatus.Change,
                    IsResolved = false,
                    ErrorCode = DataLockErrorCode.Dlock07
                }
            };

            MockDataLockRepository.Setup(x => x.GetDataLocks(444, false)).ReturnsAsync(dataLocks);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.GetDataLocks(TestApprenticeship.Id, false));

            MockDataLockRepository.Verify(x =>
                x.ResolveDataLock(It.Is<IEnumerable<long>>(p => p.SequenceEqual(new List<long> {1, 2, 3}))));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.StopApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == ExampleValidRequest.DateOfChange),It.IsAny<bool?>()));
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
        public async Task ThenShouldSendAnApprenticeshipStoppedV2Event()
        {
            await Handler.Handle(ExampleValidRequest);

            MockV2EventsPublisher.Verify(x => x.PublishApprenticeshipStopped(It.IsAny<Commitment>(),It.IsAny<Apprenticeship>()), Times.Once);
        }

        [Test]
        public void ThenThrowsExceptionIfChangeDateIsNotTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            TestApprenticeship.StartDate = startDate;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }
    }
}