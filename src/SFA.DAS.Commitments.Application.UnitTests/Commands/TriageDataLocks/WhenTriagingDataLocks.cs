using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.TriageDataLocks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
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
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private TriageDataLockCommand _validCommand;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<TriageDataLockCommand>>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _dataLockRepository = new Mock<IDataLockRepository>();

            _validator.Setup(x => x.Validate(It.IsAny<TriageDataLockCommand>()))
                .Returns(() => new ValidationResult());

            _dataLockRepository.Setup(m => m.GetDataLocks(It.IsAny<long>()))
                .ReturnsAsync(new List<DataLockStatus>
                                  {
                                      new DataLockStatus
                                          {
                                              ApprenticeshipId = 10082,
                                              DataLockEventId = 1,
                                              ErrorCode = DataLockErrorCode.Dlock07
                                          }
                                  });            

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship());

            _validCommand = new TriageDataLockCommand
            {
                ApprenticeshipId = 10082,
                TriageStatus = TriageStatus.Change,
                UserId = "testuser"
            };

            _sut = new TriageDataLockCommandHandler(
                _validator.Object,
                _dataLockRepository.Object,
                _apprenticeshipRepository.Object);
        }

        [Test]
        public async Task ShouldWork()
        {
            await _sut.Handle(_validCommand);
            _dataLockRepository.Verify(m => m.UpdateDataLockTriageStatus(It.IsAny<IEnumerable<long>>(), TriageStatus.Change), Times.Once);
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
        public async Task ShouldIgnorePassedDatalocks()
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(It.IsAny<long>()))
                .ReturnsAsync(new List<DataLockStatus>{
                                      new DataLockStatus { DataLockEventId = 1, ErrorCode = DataLockErrorCode.Dlock07, Status = Status.Pass},
                                      new DataLockStatus { DataLockEventId = 2, ErrorCode = DataLockErrorCode.Dlock03, Status = Status.Fail },
                                      new DataLockStatus { DataLockEventId = 3, ErrorCode = DataLockErrorCode.Dlock07, Status = Status.Pass }
                    });

            await _sut.Handle(_validCommand);
            _dataLockRepository.Verify(m => 
                m.UpdateDataLockTriageStatus(It.Is<IEnumerable<long>>(ids => ids.All(dataLockEventId => dataLockEventId == 2)), TriageStatus.Change)
                , Times.Once);
        }

        [TestCase(false, 1, 2, 3, Description = "Should update all datalocks if Apprenticehsip has not had any successful datalocks yet")]
        [TestCase(true, 1, 3, Description = "Should not update datalocks with course if Apprenticeship has had datalock success")]
        public async Task ShouldNotUpdateCourseDataLockIfApprenticeshipHasHadSuccessfulDataLock(bool hasHasDatalockSuccess, params long[] expectedIds)
        {
            _dataLockRepository.Setup(m => m.GetDataLocks(It.IsAny<long>()))
                .ReturnsAsync(new List<DataLockStatus>{
                                      new DataLockStatus { DataLockEventId = 1, ErrorCode = DataLockErrorCode.Dlock07 },
                                      new DataLockStatus { DataLockEventId = 2, ErrorCode = DataLockErrorCode.Dlock03 },
                                      new DataLockStatus { DataLockEventId = 3, ErrorCode = DataLockErrorCode.Dlock07 }
                                });

            long[] idsToBeUpdated = null;
            _dataLockRepository.Setup(
                m => m.UpdateDataLockTriageStatus(It.IsAny<IEnumerable<long>>(), It.IsAny<TriageStatus>()))
                .Callback<IEnumerable<long>, TriageStatus>(
                    (ids, triageStatus) => { idsToBeUpdated = ids.ToArray(); })
                .ReturnsAsync(0);

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship { HasHadDataLockSuccess = hasHasDatalockSuccess });

            await _sut.Handle(_validCommand);
            
            idsToBeUpdated.ShouldBeEquivalentTo(expectedIds);
        }
    }
}