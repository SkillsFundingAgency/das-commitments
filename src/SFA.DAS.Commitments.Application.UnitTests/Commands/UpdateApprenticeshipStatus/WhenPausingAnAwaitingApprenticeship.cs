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
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenPausingAnAwaitingApprenticeship : WhenPausingAnApprenticeshipBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            ExampleValidRequest = new PauseApprenticeshipCommand
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
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            MockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            MockApprenticeshipRespository
                .Setup(x => x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository
                .Setup(x => x.UpdateApprenticeshipStatus(TestApprenticeship.CommitmentId,
                    ExampleValidRequest.ApprenticeshipId,
                    PaymentStatus.Paused))
                .Returns(Task.FromResult(new object()));

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == TestApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
                {
                    Id = 123L,
                    EmployerAccountId = ExampleValidRequest.AccountId
                });
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
                    logger.Info($"Employer: {ExampleValidRequest.AccountId} has called PauseApprenticeshipCommand",
                        ExampleValidRequest.AccountId,
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        ExampleValidRequest.ApprenticeshipId,
                        It.IsAny<int?>(),
                        ExampleValidRequest.Caller)
                , Times.Once);
        }


        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            await Handler.Handle(ExampleValidRequest);

            MockApprenticeshipRespository.Verify(x => x.PauseApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                It.Is<DateTime>(a => a == ExampleValidRequest.DateOfChange)
            ));
        }

        [Test]
        public async Task ThenShouldSendAnApprenticeshipEvent()
        {
            await Handler.Handle(ExampleValidRequest);

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(
                It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>(),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused),
                It.Is<DateTime?>(a => a.Equals(ExampleValidRequest.DateOfChange)),
                null));
        }

        [Test]
        public void ThenThrowsExceptionIfChangeDateNotEqualToCurrentDate()
        {
            ExampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should()
                .Be("Invalid Date of Change. Date should be todays date.");
        }
    }
}