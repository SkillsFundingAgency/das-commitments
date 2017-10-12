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
    public sealed class WhenResumingAStartedApprenticeship : WhenResumingAnApprenticeship
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            SetupCommonAcademicYear();

            PauseAndResumeCurrentAcademicYear();

            MockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(
                        It.Is<long>(c => c == TestApprenticeship.CommitmentId),
                        It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                        It.Is<PaymentStatus>(s => s == PaymentStatus.Active)))
                .Returns(Task.FromResult(new object()));


            MockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == TestApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
                {
                    Id = 123L,
                    EmployerAccountId = ExampleValidRequest.AccountId
                });
        }


        private void SetupCommonAcademicYear()
        {
            MockAcademicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate)
                .Returns(new DateTime(2015, 8, 1));
            MockAcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate)
                .Returns(new DateTime(2016, 7, 31));
            MockAcademicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod)
                .Returns(new DateTime(2016, 10, 19, 18, 0, 0, 0));
        }

        private void PauseAndResumeCurrentAcademicYear()
        {
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2016, 6, 1));

            var startDate = MockAcademicYearDateProvider.Object.CurrentAcademicYearStartDate.Date;
            var pauseDate = startDate.AddMonths(5);

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Paused,
                StartDate = startDate,
                PauseDate = pauseDate
            };

            ExampleValidRequest = new ResumeApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = MockCurrentDateTime.Object.Now.Date,
                UserName = "Bob"
            };
        }

        private void PauseInLastAcademicYearResumeAfterCutoff()
        {
            MockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(MockAcademicYearDateProvider.Object
                    .LastAcademicYearFundingPeriod.AddDays(-1));

            var startDate = MockAcademicYearDateProvider.Object
                .CurrentAcademicYearStartDate.Date.AddMonths(-6);
            var pauseDate = startDate.AddMonths(3);

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Paused,
                StartDate = startDate,
                PauseDate = pauseDate
            };

            ExampleValidRequest = new ResumeApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = MockCurrentDateTime.Object.Now.Date,
                UserName = "Bob"
            };
        }

        private void PauseInLastAcademicYearBeforeCutoff()
        {
            MockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(MockAcademicYearDateProvider.Object
                    .LastAcademicYearFundingPeriod.AddDays(1));

            var startDate = MockAcademicYearDateProvider.Object.CurrentAcademicYearStartDate.Date;
            var pauseDate = startDate.AddMonths(5);

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Paused,
                StartDate = startDate,
                PauseDate = pauseDate
            };

            ExampleValidRequest = new ResumeApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = MockCurrentDateTime.Object.Now.Date,
                UserName = "Bob"
            };
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
                    logger.Info($"Employer: {ExampleValidRequest.AccountId} has called ResumeApprenticeshipCommand",
                        ExampleValidRequest.AccountId,
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        ExampleValidRequest.ApprenticeshipId,
                        It.IsAny<int?>(),
                        ExampleValidRequest.Caller)
                , Times.Once);
        }


        [Test]
        public async Task ThenShouldSendAnApprenticeshipEventWithDateOfChange()
        {
            await Handler.Handle(ExampleValidRequest);

            MockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(
                It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>(),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Active),
                It.Is<DateTime?>(a => a.Equals(ExampleValidRequest.DateOfChange.Date)),
                null));
        }

        [Test]
        public void ThenThrowsExceptionIfChangeDateIsNotDateOfChange()
        {
            ExampleValidRequest.DateOfChange = MockCurrentDateTime.Object.Now.Date.AddDays(1);

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should()
                .Be("Invalid Date of Change. Date should be todays date.");
        }
    }
}