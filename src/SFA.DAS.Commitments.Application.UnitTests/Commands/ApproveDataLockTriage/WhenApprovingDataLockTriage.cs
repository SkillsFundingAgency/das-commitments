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
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

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

        private ApproveDataLockTriageCommand _command;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<ApproveDataLockTriageCommand>>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();

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

            _sut = new ApproveDataLockTriageCommandHandler(
            _validator.Object, 
            _dataLockRepository.Object, 
            _apprenticeshipRepository.Object,
            Mock.Of<IApprenticeshipEventsPublisher>(),
            Mock.Of<IApprenticeshipEventsList>(),
            _commitmentRepository.Object,
            Mock.Of<ICurrentDateTime>());
        }

        [Test]
        public async Task ShouldCallApprenticeshipRepositoryToGetDataForPublishingEvent()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
                .Returns(new ValidationResult { Errors = { new ValidationFailure("Error", "Oh no!")}});

            Func<Task> act = () => _sut.Handle(_command);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ShouldNotUpdatePriceIfNoNewFromDataLock()
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>());

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.Is<IEnumerable<PriceHistory>>(ph => AssertPriceHistory(ph, 0))),
                    Times.Never);
        }

        [Test]
        public async Task ShouldNotUpdateWhenDataLockIsUnhandled()
        {
            var isResolvedDataLock = new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = _command.ApprenticeshipId, IsResolved = true, Status = Status.Fail,  IlrTotalCost = 505,
                        ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, TriageStatus = TriageStatus.Change};
            var isPassedDataLock = new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Pass, IlrTotalCost = 499,
                        ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now};
            var toBeUpdateDataLock = new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400,
                        ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now.AddMonths(1), TriageStatus = TriageStatus.Change};

            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus> { isResolvedDataLock, isPassedDataLock, toBeUpdateDataLock });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>())).Callback<IEnumerable<long>>( (ids) => { idsToBeUpdated = ids.ToArray(); })
                .ReturnsAsync(0);

            IEnumerable<PriceHistory> prices = null;
            _apprenticeshipRepository.Setup(
                m => m.InsertPriceHistory(_command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()))
                .Callback<long, IEnumerable<PriceHistory>>((i, l) => prices = l)
                .Returns(Task.FromResult(1L));

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(m => m.InsertPriceHistory( _command.ApprenticeshipId, It.IsAny<IEnumerable<PriceHistory>>()), Times.Once);

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
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
                .ReturnsAsync(0);

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
        public async Task ShouldNotUpdateWhenDataLockHasCourseAndApprenticeshipHashadDataLockSuccess(bool hasHadDatalockSuccess)
        {
            var isDataLockWithCourse = new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail,  IlrTotalCost = 505,
                        ErrorCode = DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock03, IlrEffectiveFromDate = DateTime.Now, TriageStatus = TriageStatus.Change};

            var toBeUpdateDataLock = new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400,
                        ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now.AddMonths(1), TriageStatus = TriageStatus.Change};


            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus> { isDataLockWithCourse, toBeUpdateDataLock });

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(_command.ApprenticeshipId))
                .ReturnsAsync(new Apprenticeship { CommitmentId = 123456L, HasHadDataLockSuccess = hasHadDatalockSuccess });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>())).Callback<IEnumerable<long>>( (ids) => { idsToBeUpdated = ids.ToArray(); })
                .ReturnsAsync(0);

            await _sut.Handle(_command);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Once);
            if (hasHadDatalockSuccess)
                idsToBeUpdated.Should().NotContain(isDataLockWithCourse.DataLockEventId, because: "Should not update when apprenticeship has had a datalock success");
            else
                idsToBeUpdated.Should().Contain(isDataLockWithCourse.DataLockEventId, because: "Should not update when apprenticeship has NOT had a datalock success"); 

            idsToBeUpdated.Should().Contain(toBeUpdateDataLock.DataLockEventId, because: "Should update datalocks with triage status 'change' that is not passed or resolved");
        }

        [Test]
        public async Task ShouldSetEndDateForNewPriceHistory()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
        public async Task ShouldSetEndDateForNewPriceHistoryOneRecord()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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

        private bool AssertPriceHistory(IEnumerable<PriceHistory> ph, int expectedTotal)
        {
            return
                    ph.Count() == expectedTotal
                && !ph.Any(m => m.Cost > 500);
        }
    }
}
