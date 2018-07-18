using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStopDate
{
    [TestFixture]
    public sealed class WhenEditingTheStopDateOfAnApprenticeship : WhenUpdatingAnApprenticeshipStopDate
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();


            ExampleValidRequest = new UpdateApprenticeshipStopDateCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                StopDate = DateTime.UtcNow.Date,
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                Id = 444L,
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Withdrawn,
                StopDate = DateTime.UtcNow.Date.AddMonths(3),
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
            MockDataLockRepository.Setup(x => x.GetDataLocks(ExampleValidRequest.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>());

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = ExampleValidRequest.AccountId
            });

            MockApprenticeshipEventsPublisher.Setup(x => x.Publish(It.IsAny<IApprenticeshipEventsList>()))
                .Returns(Task.FromResult(new Unit()));
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
                                y.First().ChangeType == ApprenticeshipChangeType.ChangeOfStopDate.ToString() &&
                                y.First().CommitmentId == null &&
                                y.First().ApprenticeshipId == TestApprenticeship.Id &&
                                y.First().OriginalState == expectedOriginalApprenticeshipState &&
                                y.First().UpdatedByRole == CallerType.Employer.ToString() &&
                                y.First().UpdatedState == expectedNewApprenticeshipState &&
                                y.First().UserId == ExampleValidRequest.UserId &&
                                y.First().ProviderId == TestApprenticeship.ProviderId &&
                                y.First().EmployerAccountId  == TestApprenticeship.EmployerAccountId  &&
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
        public async Task ThenShouldCallTheRepositoryToUpdateTheStopDate()
        {
            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStopDate(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == ExampleValidRequest.StopDate)));
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsBeforeTrainingStartDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;


            ExampleValidRequest.StopDate = startDate.AddDays(-5).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-22).Date;
            TestApprenticeship.StartDate = startDate;

            ExampleValidRequest.StopDate = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Contains("Invalid Date of Change");
        }

        [Test]
        public void ThenThrowsExceptionIfNewStopDateIsAfterCurrentStopDate()
        {
            TestApprenticeship.StopDate = ExampleValidRequest.StopDate.AddDays(-1);

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Equals("Invalid Date of Change. Date must be before current stop date.");
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            ExampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsNotStopped()
        {
            TestApprenticeship.PaymentStatus = PaymentStatus.Active;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Equals("Apprenticeship must be stopped in order to update stop date");
        }

        [Test]
        public async Task ThenShouldPublishApprenticeshipEvent()
        {
            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipEventsPublisher.Verify(x =>
                x.Publish(It.Is<ApprenticeshipEventsList>(list =>
                    list.Events.Count == 1 &&
                    list.Events[0].Apprenticeship.Id == ExampleValidRequest.ApprenticeshipId &&
                    list.Events[0].EffectiveFrom == ExampleValidRequest.StopDate
                    )), Times.Once);
        }

        [Test]
        public async Task ThenShouldResolveDataLocksLinkedToAppriceshipWhenStopDateEqualsStartDate()
        {
            var listOfDataLockStatuses = new List<DataLockStatus>
            {
                new DataLockStatus {DataLockEventId = 1},
                new DataLockStatus {DataLockEventId = 2}
            };
            ExampleValidRequest.StopDate = TestApprenticeship.StartDate.Value;
            MockDataLockRepository.Setup(x => x.GetDataLocks(It.IsAny<long>(), false)).ReturnsAsync(listOfDataLockStatuses);

            await Handler.Handle(ExampleValidRequest);

            MockDataLockRepository.Verify(x => x.GetDataLocks(TestApprenticeship.Id, false));
            MockDataLockRepository.Verify(x => x.ResolveDataLock(It.Is<IEnumerable<long>>(p => p.SequenceEqual(new [] {1L, 2L}))));
        }
    }
}