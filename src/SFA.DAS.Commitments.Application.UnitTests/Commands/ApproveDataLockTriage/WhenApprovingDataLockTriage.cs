using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.ApproveDataLockTriage
{
    [TestFixture]
    public class WhenApprovingDataLock
    {
        private ApproveDataLockTriageCommandHandler _sut;
        private Mock<AbstractValidator<ApproveDataLockTriageCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipInfoService> _apprenticeshipTrainingService;
        private Mock<IV2EventsPublisher> _v2EventsPublisher;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private ApproveDataLockTriageCommand _command;
        private List<IApprenticeshipEvent> _eventsToPublish;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<ApproveDataLockTriageCommand>>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipTrainingService = new Mock<IApprenticeshipInfoService>();

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship());

            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>()))
                .ReturnsAsync(new Commitment());

            _command = new ApproveDataLockTriageCommand
            {
                ApprenticeshipId = 4321
            };

            _validator.Setup(m => m.Validate(_command))
                .Returns(new ValidationResult());

            _v2EventsPublisher = new Mock<IV2EventsPublisher>();
            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();

            _eventsToPublish = new List<IApprenticeshipEvent>();

            _apprenticeshipEventsList
                .Setup(ael => ael.Events)
                .Returns(_eventsToPublish);

            _sut = new ApproveDataLockTriageCommandHandler(
            _validator.Object,
            _dataLockRepository.Object,
            _apprenticeshipRepository.Object,
            Mock.Of<IApprenticeshipEventsPublisher>(),
            _apprenticeshipEventsList.Object,
            _commitmentRepository.Object,
            Mock.Of<ICurrentDateTime>(),
            _apprenticeshipTrainingService.Object,
            Mock.Of<ICommitmentsLogger>(),
            _v2EventsPublisher.Object);
        }

        [Test]
        public async Task ShouldCallApprenticeshipRepositoryToGetDataForPublishingEvent()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                                  });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(x => x.GetApprenticeship(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ShouldCallCommitmentRepositoryToGetDataForPublishingEvent()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>
                      {
                            new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                      });

            await _sut.Handle(_command);

            _commitmentRepository.Verify(x => x.GetCommitmentById(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public void ShouldValidateInput()
        {
            _validator.Setup(m => m.Validate(_command))
                .Returns(new ValidationResult { Errors = { new ValidationFailure("Error", "Oh no!") } });

            Func<Task> act = () => _sut.Handle(_command);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ShouldNotUpdatePriceIfNoNewFromDataLock()
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>());

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.Is<IEnumerable<PriceHistory>>(ph => AssertPriceHistory(ph, 0))),
                    Times.Never);
        }

        [Test]
        public async Task ShouldExcludeSomeDataLocksWhenUpdatingPriceHistory()
        {
            var isResolvedDataLock = new DataLockStatus
            {
                DataLockEventId = 1,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = true,
                Status = Status.Fail,
                IlrTotalCost = 505,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now,
                TriageStatus = TriageStatus.Change
            };
            var isPassedDataLock = new DataLockStatus
            {
                DataLockEventId = 2,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Pass,
                IlrTotalCost = 499,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now
            };
            var toBeUpdateDataLock = new DataLockStatus
            {
                DataLockEventId = 3,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 400,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now.AddMonths(1),
                TriageStatus = TriageStatus.Change
            };

            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus> { isResolvedDataLock, isPassedDataLock, toBeUpdateDataLock });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>())).Callback<IEnumerable<long>>((ids) => { idsToBeUpdated = ids.ToArray(); })
                .Returns(Task.CompletedTask);

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()), Times.Once);

            prices.Count().ShouldBeEquivalentTo(3);
            _dataLockRepository.Verify(m => m.ResolveDataLock(
                It.Is<IEnumerable<long>>(d => d.Contains(toBeUpdateDataLock.DataLockEventId) && d.Count() == 1)), Times.Once);

            idsToBeUpdated.Should().NotContain(isResolvedDataLock.DataLockEventId, because: "Should not update already resolved datalocks");
            idsToBeUpdated.Should().NotContain(isPassedDataLock.DataLockEventId, because: "Should not update passed datalocks");
            idsToBeUpdated.Should().Contain(toBeUpdateDataLock.DataLockEventId, because: "Should update datalocks with triage status 'change' that is not passed or resolved");
        }

        [TestCase(false, 2, 3, Description = "Should update all with triange status change")]
        [TestCase(true, 3, Description = "Should update all with triange status change and price only")]
        public async Task ShouldOnlyUpdateDataLockWithChangeStatus(bool hasHadDatalockSuccess, params long[] expectedIds)
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>
                    {
                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 301, ErrorCode = (DataLockErrorCode)76,
                            IlrEffectiveFromDate = DateTime.Now.AddMonths(2), DataLockEventId = 1, TriageStatus = TriageStatus.Unknown },

                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 302, ErrorCode = (DataLockErrorCode)76,
                            IlrEffectiveFromDate = DateTime.Now.AddMonths(2), DataLockEventId = 1, TriageStatus = TriageStatus.FixIlr },

                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 303, ErrorCode = (DataLockErrorCode)76,
                            IlrEffectiveFromDate = DateTime.Now.AddMonths(2), DataLockEventId = 1, TriageStatus = TriageStatus.Restart },

                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 401, ErrorCode = DataLockErrorCode.Dlock06,
                            IlrEffectiveFromDate = DateTime.Now.AddMonths(2), DataLockEventId = 2, TriageStatus = TriageStatus.Change},

                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07,
                            IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                    });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(
                m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()))
                .Callback<IEnumerable<long>>(
                    (ids) => { idsToBeUpdated = ids.ToArray(); })
                .Returns(Task.CompletedTask);

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship { HasHadDataLockSuccess = hasHadDatalockSuccess });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.Is<IEnumerable<PriceHistory>>(ph => AssertPriceHistory(ph, expectedIds.Length))),
                    Times.Once);

            expectedIds.Length.Should().Be(expectedIds.Length);
            expectedIds.ShouldAllBeEquivalentTo(idsToBeUpdated);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ShouldResolveDataLockWithCourse(bool hasHadDatalockSuccess)
        {
            var isDataLockWithCourse = new DataLockStatus
            {
                DataLockEventId = 1,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 505,
                ErrorCode = DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03,
                IlrEffectiveFromDate = DateTime.Now,
                TriageStatus = TriageStatus.Change
            };

            var toBeUpdateDataLock = new DataLockStatus
            {
                DataLockEventId = 3,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 400,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now.AddMonths(1),
                TriageStatus = TriageStatus.Change
            };


            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus> { isDataLockWithCourse, toBeUpdateDataLock });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, HasHadDataLockSuccess = hasHadDatalockSuccess });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>())).Callback<IEnumerable<long>>((ids) => { idsToBeUpdated = ids.ToArray(); })
                .Returns(Task.CompletedTask);

            await _sut.Handle(_command);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Once);
            if (hasHadDatalockSuccess)
                idsToBeUpdated.Should().NotContain(isDataLockWithCourse.DataLockEventId, because: "Should not update when apprenticeship has had a datalock success");
            else
                idsToBeUpdated.Should().Contain(isDataLockWithCourse.DataLockEventId, because: "Should not update when apprenticeship has NOT had a datalock success");

            idsToBeUpdated.Should().Contain(toBeUpdateDataLock.DataLockEventId, because: "Should update datalocks with triage status 'change' that is not passed or resolved");
        }

        [Test]
        public async Task ShouldNotUpdateApprenticeshipIfApprenticeshipHasHadSuccessDataLock()
        {
            var isDataLockWithCourse = new DataLockStatus
            {
                DataLockEventId = 1,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 505,
                ErrorCode = DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03,
                IlrEffectiveFromDate = DateTime.Now,
                TriageStatus = TriageStatus.Change
            };

            var toBeUpdateDataLock = new DataLockStatus
            {
                DataLockEventId = 3,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 400,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now.AddMonths(1),
                TriageStatus = TriageStatus.Change
            };


            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus> { isDataLockWithCourse, toBeUpdateDataLock });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, HasHadDataLockSuccess = true });

            await _sut.Handle(_command);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Once);


            _apprenticeshipTrainingService.Verify(m => m.GetTrainingProgram(It.IsAny<string>()), Times.Never);
            _apprenticeshipRepository.Verify(m => m.UpdateApprenticeship(It.IsAny<Apprenticeship>(), new Caller()), Times.Never);
        }


        [Test]
        public async Task ShouldUpdateApprenticeshipIfCourseHasChanged()
        {
            var trainingCode = 10123;
            var standard = new Standard { Title = "Standard 1", Code = trainingCode, CourseName = "Standard 1 Course Name", Level = 1 };

            var isDataLockWithCourse = new DataLockStatus
            {
                DataLockEventId = 1,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 505,
                ErrorCode = DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03,
                IlrEffectiveFromDate = DateTime.Now,
                TriageStatus = TriageStatus.Change,
                IlrTrainingCourseCode = $"{trainingCode}"
            };

            var toBeUpdateDataLock = new DataLockStatus
            {
                DataLockEventId = 3,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 400,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now.AddMonths(1),
                TriageStatus = TriageStatus.Change
            };

            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus> { isDataLockWithCourse, toBeUpdateDataLock });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, HasHadDataLockSuccess = false, EmployerAccountId = 12345 });

            _apprenticeshipTrainingService.Setup(m => m.GetTrainingProgram($"{trainingCode}"))
                .ReturnsAsync(standard);

            Apprenticeship updatedApprenticeship = null;
            _apprenticeshipRepository.Setup(
                m => m.UpdateApprenticeship(It.IsAny<Apprenticeship>(), It.IsAny<Caller>()))
                .Callback<Apprenticeship, Caller>((a, c) => updatedApprenticeship = a)
                .Returns(Task.FromResult(0));

            await _sut.Handle(_command);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Once);

            _apprenticeshipTrainingService.Verify(m => m.GetTrainingProgram(standard.Code.ToString()), Times.Once);
            _apprenticeshipRepository.Verify(m => m.UpdateApprenticeship(It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);

            updatedApprenticeship.TrainingCode.Should().Be(standard.Code.ToString());
            updatedApprenticeship.TrainingName.Should().Be(standard.Title);
            updatedApprenticeship.TrainingType.Should().Be(TrainingType.Standard);
        }

        [Test]
        public async Task ShouldNotUpdateApprenticeshipIfCourseIsTheSame()
        {
            var trainingCode = 10123;
            var standard = new Standard { Title = "Standard 1", Code = trainingCode, CourseName = "Standard 1 Course Name", Level = 1 };

            var isDataLockWithCourse = new DataLockStatus
            {
                DataLockEventId = 1,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 505,
                ErrorCode = DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03,
                IlrEffectiveFromDate = DateTime.Now,
                TriageStatus = TriageStatus.Change,
                IlrTrainingCourseCode = $"{trainingCode}"
            };

            var toBeUpdateDataLock = new DataLockStatus
            {
                DataLockEventId = 3,
                ApprenticeshipId = _command.ApprenticeshipId,
                IsResolved = false,
                Status = Status.Fail,
                IlrTotalCost = 400,
                ErrorCode = DataLockErrorCode.Dlock07,
                IlrEffectiveFromDate = DateTime.Now.AddMonths(1),
                TriageStatus = TriageStatus.Change,
                IlrTrainingCourseCode = $"{trainingCode}"
            };

            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus> { isDataLockWithCourse, toBeUpdateDataLock });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, HasHadDataLockSuccess = false, EmployerAccountId = 12345, TrainingCode = $"{trainingCode}" });

            _apprenticeshipTrainingService.Setup(m => m.GetTrainingProgram($"{trainingCode}"))
                .ReturnsAsync(standard);

            await _sut.Handle(_command);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Once);

            _apprenticeshipTrainingService.Verify(m => m.GetTrainingProgram(It.IsAny<string>()), Times.Never);
            _apprenticeshipRepository.Verify(m => m.UpdateApprenticeship(It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
        }


        [Test]
        public async Task ShouldSetEndDateForNewPriceHistory()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                    .ReturnsAsync(new List<DataLockStatus>
                      {
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1500,
                                          ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-06-01"),  DataLockEventId = 1, TriageStatus = TriageStatus.Change},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1600,
                                          ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-07-01"), DataLockEventId = 2, TriageStatus = TriageStatus.Change},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1700,
                                          ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-12-01"), DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                      });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L });

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            await _sut.Handle(_command);

            var p1 = prices.Single(m => m.Cost == 1500);
            var p2 = prices.Single(m => m.Cost == 1600);
            var p3 = prices.Single(m => m.Cost == 1700);

            p1.ToDate.Should().Be(p2.FromDate.AddDays(-1));
            p2.ToDate.Should().Be(p3.FromDate.AddDays(-1));
            p3.ToDate.Should().Be(null);
        }

        [Test]
        public async Task ShouldCreatePriceHistoryForCourseAndPriceDataLock()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                    .ReturnsAsync(new List<DataLockStatus>
                      {
                                      // Price / progCode / framework
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1500,
                                          ErrorCode = (DataLockErrorCode)76, IlrEffectiveFromDate = DateTime.Parse("2017-06-01"),  DataLockEventId = 1, TriageStatus = TriageStatus.Change},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1600,
                                          ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-07-01"), DataLockEventId = 2, TriageStatus = TriageStatus.Change},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1700,
                                          ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-12-01"), DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                      });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L });

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            await _sut.Handle(_command);

            var p1 = prices.Single(m => m.Cost == 1500);
            var p2 = prices.Single(m => m.Cost == 1600);
            var p3 = prices.Single(m => m.Cost == 1700);

            p1.ToDate.Should().Be(p2.FromDate.AddDays(-1));
            p2.ToDate.Should().Be(p3.FromDate.AddDays(-1));
            p3.ToDate.Should().Be(null);
        }


        [Test]
        public async Task ShouldCreatePriceHistoryWithDistinctCostAndDates()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                    .ReturnsAsync(new List<DataLockStatus>
                      {
                                      new DataLockStatus 
                                      {
                                          DataLockEventId = 1,
                                          DataLockEventDatetime = DateTime.Parse("2020-10-23"),
                                          PriceEpisodeIdentifier = "25-362-01/07/2020",
                                          ApprenticeshipId = _command.ApprenticeshipId,
                                          IlrTrainingCourseCode = "362",
                                          IlrTrainingType = 0,
                                          IlrActualStartDate = DateTime.Parse("2020-07-01"),
                                          IlrEffectiveFromDate = DateTime.Parse("2020-07-01"),
                                          IlrPriceEffectiveToDate = DateTime.Parse("2020-07-31"),
                                          IlrTotalCost = 18291,
                                          ErrorCode = (DataLockErrorCode)0,
                                          Status = Status.Pass,
                                          TriageStatus = TriageStatus.Unknown,
                                          ApprenticeshipUpdateId = null,
                                          IsResolved = false,
                                          EventStatus = EventStatus.Updated,
                                          IsExpired = false,
                                          Expired = null
                                      },
                                      new DataLockStatus
                                      {
                                          DataLockEventId = 2,
                                          DataLockEventDatetime = DateTime.Parse("2021-06-04"),
                                          PriceEpisodeIdentifier = "25-362-01/08/2020",
                                          ApprenticeshipId = _command.ApprenticeshipId,
                                          IlrTrainingCourseCode = "362",
                                          IlrTrainingType = 0,
                                          IlrActualStartDate = DateTime.Parse("2020-08-01"),
                                          IlrEffectiveFromDate = DateTime.Parse("2020-07-01"),
                                          IlrPriceEffectiveToDate = DateTime.Parse("2021-02-04"),
                                          IlrTotalCost = 18291,
                                          ErrorCode = (DataLockErrorCode)0,
                                          Status = Status.Pass,
                                          TriageStatus = TriageStatus.Unknown,
                                          ApprenticeshipUpdateId = null,
                                          IsResolved = false,
                                          EventStatus = EventStatus.New,
                                          IsExpired = false,
                                          Expired = null
                                      },
                                      new DataLockStatus
                                      {
                                          DataLockEventId = 3,
                                          DataLockEventDatetime = DateTime.Parse("2021-06-04"),
                                          PriceEpisodeIdentifier = "25-362-05/02/2021",
                                          ApprenticeshipId = _command.ApprenticeshipId,
                                          IlrTrainingCourseCode = "362",
                                          IlrTrainingType = 0,
                                          IlrActualStartDate = DateTime.Parse("2021-02-05"),
                                          IlrEffectiveFromDate = DateTime.Parse("2021-02-05"),
                                          IlrPriceEffectiveToDate = DateTime.Parse("2021-07-31"),
                                          IlrTotalCost = 16725,
                                          ErrorCode = (DataLockErrorCode)0,
                                          Status = Status.Fail,
                                          TriageStatus = TriageStatus.Change,
                                          ApprenticeshipUpdateId = null,
                                          IsResolved = false,
                                          EventStatus = EventStatus.Updated,
                                          IsExpired = false,
                                          Expired = null
                                      },
                      });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, TrainingCode = "362" });

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            // Act 
            await _sut.Handle(_command);

            // Assert
            prices.Should().HaveCount(2);
        }

        [Test]
        public async Task ShouldSetEndDateForNewPriceHistoryOneRecord()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                    .ReturnsAsync(new List<DataLockStatus>
                      {
                        new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 1500,
                            ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Parse("2017-06-01"),  DataLockEventId = 1, TriageStatus = TriageStatus.Change}
                      });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L });

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            await _sut.Handle(_command);

            var p1 = prices.Single(m => m.Cost == 1500);

            p1.ToDate.Should().Be(null);
        }


        [Test]
        public async Task ShouldPublishDataLockTriageApprovedEvent()
        {
            var mockEvent = new Mock<IApprenticeshipEvent>();
            mockEvent.Setup(ev => ev.Apprenticeship).Returns(new Apprenticeship { Id = _command.ApprenticeshipId });
            mockEvent.Setup(ev => ev.Commitment).Returns(new Commitment());
            _eventsToPublish.Add(mockEvent.Object);
            _eventsToPublish.Add(mockEvent.Object);

            _apprenticeshipEventsList
                .Setup(ael => ael.Events)
                .Returns(_eventsToPublish);

            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>
                {
                    new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                });

            await _sut.Handle(_command);

            _v2EventsPublisher.Verify(x => x.PublishDataLockTriageApproved(It.Is<IApprenticeshipEvent>(ev => ev.Apprenticeship.Id == _command.ApprenticeshipId)), Times.Exactly(_eventsToPublish.Count));
        }

        private bool AssertPriceHistory(IEnumerable<PriceHistory> ph, int expectedTotal)
        {
            return
                    ph.Count() == expectedTotal
                && !ph.Any(m => m.Cost > 500);
        }
    }
}
