using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;


namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetDataLocks
{
    [TestFixture()]
    public class WhenGettingDataLocks
    {
        private GetDataLocksQueryHandler _handler;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<AbstractValidator<GetDataLocksRequest>> _validator;

        [SetUp]
        public void Arrange()
        {
            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(x => x.GetDataLocks(It.IsAny<long>()))
                .ReturnsAsync(new List<DataLockStatus>());

            _validator = new Mock<AbstractValidator<GetDataLocksRequest>>();
            _validator.Setup(x => x.Validate(It.IsAny<GetDataLocksRequest>()))
                .Returns(() => new ValidationResult());

            _handler = new GetDataLocksQueryHandler(_validator.Object, _dataLockRepository.Object);
        }

        [Test]
        public async Task TheTheRepositoryShouldBeCalledToRetrieveData()
        {
            //Arrange
            var request = new GetDataLocksRequest();

            //Act
            await _handler.Handle(request);

            //Assert
            _dataLockRepository.Verify(x => x.GetDataLocks(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRequestShouldBeValidated()
        {
            //Arrange
            var request = new GetDataLocksRequest();

            //Act
            await _handler.Handle(request);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<GetDataLocksRequest>()), Times.Once);
        }


        [Test]
        public async Task ThenTheResultShouldBeMapped()
        {
            //Arrange
            var dataLockStatus = new DataLockStatus
            {
                DataLockEventId = 1L,
                DataLockEventDatetime = new DateTime(2018,3,1),
                PriceEpisodeIdentifier = "PRICE_EPISODE_ID",
                ApprenticeshipId = 999L,
                IlrTrainingCourseCode = "TRAINING_COURSE_CODE",
                IlrTrainingType = TrainingType.Framework,
                IlrActualStartDate = new DateTime(2018,1,1),
                IlrEffectiveFromDate = new DateTime(2018,12,31),
                IlrTotalCost = decimal.One,
                Status  = Status.Fail,
                TriageStatus = TriageStatus.Change,
                ErrorCode = DataLockErrorCode.Dlock01
            };

            _dataLockRepository.Setup(x => x.GetDataLocks(It.IsAny<long>()))
                .ReturnsAsync(new List<DataLockStatus>
                    {
                        dataLockStatus
                    });

            var request = new GetDataLocksRequest();

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);

            var dataLockResult = result.Data[0];
            Assert.AreEqual(dataLockStatus.DataLockEventId, dataLockResult.DataLockEventId);
            Assert.AreEqual(dataLockStatus.DataLockEventDatetime, dataLockResult.DataLockEventDatetime);
            Assert.AreEqual(dataLockStatus.PriceEpisodeIdentifier, dataLockResult.PriceEpisodeIdentifier);
            Assert.AreEqual(dataLockStatus.ApprenticeshipId, dataLockResult.ApprenticeshipId);
            Assert.AreEqual(dataLockStatus.IlrTrainingCourseCode, dataLockResult.IlrTrainingCourseCode);
            Assert.AreEqual(dataLockStatus.IlrTrainingType, (TrainingType)dataLockResult.IlrTrainingType);
            Assert.AreEqual(dataLockStatus.IlrActualStartDate, dataLockResult.IlrActualStartDate);
            Assert.AreEqual(dataLockStatus.IlrEffectiveFromDate, dataLockResult.IlrEffectiveFromDate);
            Assert.AreEqual(dataLockStatus.IlrTotalCost, dataLockResult.IlrTotalCost);
            Assert.AreEqual(dataLockStatus.Status, (Status)dataLockResult.Status);
            Assert.AreEqual(dataLockStatus.TriageStatus, (TriageStatus)dataLockResult.TriageStatus);
            Assert.AreEqual(dataLockStatus.ErrorCode, (DataLockErrorCode)dataLockResult.ErrorCode);
        }
    }
}
