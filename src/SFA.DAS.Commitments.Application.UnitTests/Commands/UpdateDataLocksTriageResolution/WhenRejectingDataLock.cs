using System;
using System.Threading.Tasks;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution;
using SFA.DAS.Commitments.Domain.Data;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using DataLockUpdateType = SFA.DAS.Commitments.Api.Types.DataLock.Types.DataLockUpdateType;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateDataLocksTriageResolution
{
    [TestFixture]
    public class WhenRejectingDataLock
    {
        private UpdateDataLocksTriageResolutionHandler _sut;
        private Mock<AbstractValidator<UpdateDataLocksTriageResolutionCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        private UpdateDataLocksTriageResolutionCommand _command;

        private Mock<ICommitmentRepository> _commitmentRepository;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<UpdateDataLocksTriageResolutionCommand>>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _commitmentRepository = new Mock<ICommitmentRepository>();

            _command = new UpdateDataLocksTriageResolutionCommand
            {
                DataLockUpdateType = DataLockUpdateType.RejectChanges,
                ApprenticeshipId = 4321
            };

            _validator.Setup(m => m.Validate(_command))
                .Returns(new ValidationResult());

            _sut = new UpdateDataLocksTriageResolutionHandler(
            _validator.Object,
            _dataLockRepository.Object,
            _apprenticeshipRepository.Object,
            Mock.Of<IApprenticeshipEventsPublisher>(),
            Mock.Of<IApprenticeshipEventsList>(),
            _commitmentRepository.Object,
            Mock.Of<ICurrentDateTime>());
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
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
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
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = true, IlrTotalCost = 505, ErrorCode = DataLockErrorCode.Dlock07, DataLockEventId = 1},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Pass, IlrTotalCost = 506, ErrorCode = DataLockErrorCode.Dlock07, DataLockEventId = 2},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, IsResolved = false, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3}
                                  });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.IsAny<IEnumerable<PriceHistory>>()),
                    Times.Never);
            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Never());

            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.Is<IEnumerable<long>>(d => d.Contains(3L) && d.Count() == 1), TriageStatus.Unknown), Times.Once);
        }


        [Test]
        public async Task ShouldOnlyResetUpdateDataLockWithPrice()
        {
            Debug.Assert(_dataLockRepository != null, "_dataLockRepository != null");
            _dataLockRepository.Setup(m => m.GetDataLocks(_command.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 505, ErrorCode = (DataLockErrorCode)76, DataLockEventId = 1},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 506, ErrorCode = DataLockErrorCode.Dlock06, DataLockEventId = 2},
                                      new DataLockStatus { ApprenticeshipId = _command.ApprenticeshipId, Status = Status.Fail, IlrTotalCost = 400, ErrorCode = DataLockErrorCode.Dlock07, IlrEffectiveFromDate = DateTime.Now, DataLockEventId = 3}
                                  });

            await _sut.Handle(_command);

            _apprenticeshipRepository.Verify(
                m => m.InsertPriceHistory(
                    _command.ApprenticeshipId,
                    It.IsAny<IEnumerable<PriceHistory>>()),
                    Times.Never);
            _dataLockRepository.Verify(m => m.ResolveDataLock(It.IsAny<IEnumerable<long>>()), Times.Never());

            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.Is<IEnumerable<long>>(d => d.Contains(3L) && d.Count() == 1), TriageStatus.Unknown), Times.Once);
        }
    }
}
