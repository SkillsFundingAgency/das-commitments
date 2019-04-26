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
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
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


            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 6, 1));

            var startDate = MockCurrentDateTime.Object.Now.Date.AddMonths(-6);
            var pauseDate = startDate.AddMonths(1);

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
                DateOfChange = pauseDate.Date,
                UserName = "Bob"
            };

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
        public async Task ThenShouldPublishAV2ApprenticeshipResumedEvent()
        {
            await Handler.Handle(ExampleValidRequest);

            MockV2EventsPublisher.Verify(x => x.PublishApprenticeshipResumed(
                It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>()));
        }


        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-8-1", "2017-9-1", "2017-9-1", "2017-10-1", true, false ,"Inside Academic year, and is correct")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-8-1", "2017-9-1", "2017-10-1", "2017-10-1", false, false, "Inside Academic year and is not correct before R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-6-1", "2017-8-1", true, false, "Paused Last Academic year and is correct before R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-6-1", "2017-10-18", true, false, "Paused Last Academic year and is correct before R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-1-1", "2017-10-18", false, false, "Paused Last Academic year and is not correct before R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-6-1", "2017-10-20", false, true, "Paused Last Academic year and is not correct  after R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-1-1", "2017-10-20", false, true, "Paused Last Academic year and is not correct after R14 Cutoff")]
        [TestCase("2017-8-1", "2018-7-31", "2017-10-19 18:00:00", "2017-1-1", "2017-6-1", "2017-8-1", "2017-10-20", true, false, "Paused Last Academic year and is correct after R14 Cutoff")]
        public void ThenItValidatesDataofChangeAccordingToAcademicYearRule(
            DateTime academicYearStart,
            DateTime academicYearEnd,
            DateTime academicYearR14Cutoff,
            DateTime startDate,
            DateTime pausedDate,
            DateTime resumeDate,
            DateTime timeNow,
            bool expectToPassValidation, 
            bool validatesOnStartDate, string scenario)
        {
            MockAcademicYearDateProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(academicYearStart);
            MockAcademicYearDateProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(academicYearEnd);
            MockAcademicYearDateProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(academicYearR14Cutoff);
            
            if (timeNow > academicYearR14Cutoff)
            {
                MockAcademicYearValidator.Setup(v => v.Validate(It.IsAny<DateTime>()))
                    .Returns(AcademicYearValidationResult.NotWithinFundingPeriod);
            }
            else
            {
                MockAcademicYearValidator.Setup(v => v.Validate(It.IsAny<DateTime>()))
                    .Returns(AcademicYearValidationResult.Success);
            }

            MockCurrentDateTime.Setup(d => d.Now).Returns(timeNow);

            TestApprenticeship.PauseDate = pausedDate.Date;
            ExampleValidRequest.DateOfChange = resumeDate.Date;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            if (expectToPassValidation)
            {
                act.ShouldNotThrow<ValidationException>();
            }
            else
            {
                act.ShouldThrow<ValidationException>()
                    .Which.Message.Should().Be(validatesOnStartDate
                        ? "Invalid Date of Change. Date should be the academic year start date."
                        : "Invalid Date of Change. Date should be the pause date.");
            }
        }

    }
}