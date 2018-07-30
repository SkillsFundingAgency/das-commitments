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
using SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.RejectDataLockTriage
{
    [TestFixture]
    public class WhenRejectingDataLock
    {
        private RejectDataLockTriageCommandHandler _sut;
        private Mock<AbstractValidator<RejectDataLockTriageCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        private RejectDataLockTriageCommand _command;

        private Mock<ICommitmentRepository> _commitmentRepository;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<RejectDataLockTriageCommand>>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _commitmentRepository = new Mock<ICommitmentRepository>();

            _command = new RejectDataLockTriageCommand { ApprenticeshipId = 4321 };

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship());

            _validator.Setup(m => m.Validate(_command))
                .Returns(new ValidationResult());

            _sut = new RejectDataLockTriageCommandHandler(
            _validator.Object,
            _dataLockRepository.Object,
            _apprenticeshipRepository.Object,
            _commitmentRepository.Object);
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
                    It.IsAny<IEnumerable<PriceHistory>>()),
                    Times.Never);

            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.IsAny<IEnumerable<long>>(), It.IsAny<TriageStatus>()), Times.Never);
        }

        [Test]
        public async Task ShouldNotResetWhenDataLockIsUnhandled()
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = true, IlrTotalCost = 505, ErrorCode = DataLockErrorCode.Dlock07, DataLockEventId = 1, TriageStatus = TriageStatus.Change},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Pass, IlrTotalCost = 506, ErrorCode = DataLockErrorCode.Dlock07, DataLockEventId = 2},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3, TriageStatus = TriageStatus.Change}
                                  });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.IsAny<IEnumerable<PriceHistory>>()),
                    Times.Never);
            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Never());

            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(
                It.Is<IEnumerable<long>>(d => d.Contains(3L) && d.Count() == 1), TriageStatus.Unknown),
                Times.Once);
        }

        [TestCase(false, 2, 3, Description = "Should update all with triange status change")]
        [TestCase(true, 3, Description = "Should update all with triange status change and price only")]
        public async Task ShouldRejectDatalock(bool hasHadDatalockSuccess, params long[] expectedIds)
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId, false))
                .ReturnsAsync(
                new List<DataLockStatus>
                {
                    new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail,
                                         IlrTotalCost = 505, ErrorCode = (DataLockErrorCode)76 },

                    new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail,
                                         IlrTotalCost = 506, ErrorCode = DataLockErrorCode.Dlock06, TriageStatus = TriageStatus.Change },

                    new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail,
                                         IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now,
                                         TriageStatus = TriageStatus.Change }
                });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(
                m => m.UpdateDataLockTriageStatus(It.IsAny<IEnumerable<long>>(), It.IsAny<TriageStatus>()))
                .Callback<IEnumerable<long>, TriageStatus>(
                    (ids, triageStatus) => { idsToBeUpdated = ids.ToArray(); })
                .Returns(Task.CompletedTask);

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship { HasHadDataLockSuccess = hasHadDatalockSuccess });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.IsAny<IEnumerable<PriceHistory>>()),
                    Times.Never);

            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Never());

            idsToBeUpdated.Length.Should().Be(expectedIds.Length);
            idsToBeUpdated.ShouldAllBeEquivalentTo(expectedIds);
        }
    }
}
