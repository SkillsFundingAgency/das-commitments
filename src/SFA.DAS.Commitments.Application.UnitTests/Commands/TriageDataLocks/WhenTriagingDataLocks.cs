using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.TriageDataLocks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using DataLockErrorCode = SFA.DAS.Commitments.Domain.Entities.DataLock.DataLockErrorCode;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.TriageDataLocks
{
    [TestFixture]
    public class WhenTriagingDataLocks
    {
        private TriageDataLockCommandHandler _sut;
        private Mock<AbstractValidator<TriageDataLockCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private TriageDataLockCommand _validCommand;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<TriageDataLockCommand>>();
            _validator.Setup(x => x.Validate(It.IsAny<TriageDataLockCommand>()))
                .Returns(() => new ValidationResult());

            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(m => m.GetDataLocks(It.IsAny<long>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus
                                          {
                                              ApprenticeshipId = 10082,
                                              DataLockEventId = 1,
                                              ErrorCode = DataLockErrorCode.Dlock07
                                          }
                                  });            

            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _validCommand = new TriageDataLockCommand
            {
                ApprenticeshipId = 10082,
                TriageStatus = Domain.Entities.DataLock.TriageStatus.Change,
                UserId = "testuser"
            };

            _sut = new TriageDataLockCommandHandler(
                _validator.Object,
                _dataLockRepository.Object);
        }

        [Test]
        public async Task ShouldWork()
        {
            await _sut.Handle(_validCommand);
            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.IsAny<IEnumerable<long>>(), Domain.Entities.DataLock.TriageStatus.Change), Times.Once);
        }

        [Test]
        public void ShouldFailOnValidation()
        {
            _validator.Setup(m => m.Validate(_validCommand))
                .Returns(new ValidationResult {Errors = { new ValidationFailure("FailedToValidate", "Oh no!!")}});

            Func<Task> act = async () => await _sut.Handle(_validCommand);
            act.ShouldThrow<ValidationException>();
        }
        [Test]
        public async Task ShouldIgnoreCourseMismatch()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(It.IsAny<long>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<DataLockStatus>{
                                      new DataLockStatus
                                          {
                                              ApprenticeshipId = _validCommand.ApprenticeshipId,
                                              DataLockEventId = 1,
                                              ErrorCode = DataLockErrorCode.Dlock07
                                          },
                                      new DataLockStatus
                                          {
                                              ApprenticeshipId = _validCommand.ApprenticeshipId,
                                              DataLockEventId = 2,
                                              ErrorCode = DataLockErrorCode.Dlock03
                                          }
                });

            await _sut.Handle(_validCommand);
            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.Is<IEnumerable<long>>(q => q.All(ss => ss == 1)), Domain.Entities.DataLock.TriageStatus.Change), Times.Once);
        }
    }
}