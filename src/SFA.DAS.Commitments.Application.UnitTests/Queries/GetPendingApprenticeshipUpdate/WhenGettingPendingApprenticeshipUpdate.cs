using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetPendingApprenticeshipUpdate
{
    [TestFixture()]
    public class WhenGettingPendingApprenticeshipUpdate
    {
        private Mock<GetPendingApprenticeshipUpdateValidator> _validator;
        private Mock<IApprenticeshipUpdateRepository> _repository;

        private ApprenticeshipUpdate _testRecord;

        private GetPendingApprenticeshipUpdateQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<GetPendingApprenticeshipUpdateValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<GetPendingApprenticeshipUpdateRequest>()))
                .Returns(() => new ValidationResult());

            _testRecord = new ApprenticeshipUpdate
            {
                Id = 1,
                ApprenticeshipId = 1,
                Originator = Originator.Employer,
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(2000, 01, 03),
                ULN = "1234567890",
                TrainingType = TrainingType.Framework,
                TrainingCode = "AB-123",
                TrainingName = "Test Course",
                Cost = 1000,
                StartDate = new DateTime(2018, 1, 1),
                EndDate = new DateTime(2018, 6, 1)
            };

            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _repository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(_testRecord);

            _handler = new GetPendingApprenticeshipUpdateQueryHandler(_validator.Object, _repository.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest();

            //Act
            await _handler.Handle(request);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<GetPendingApprenticeshipUpdateRequest>()), Times.Once);
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationExceptionIsThrown()
        {
            //Arrange
            _validator.Setup(x => x.Validate(It.IsAny<GetPendingApprenticeshipUpdateRequest>()))
                .Returns(() =>
                        new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure("Error", "Error Message")
                        }));

            var request = new GetPendingApprenticeshipUpdateRequest();

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveData()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest();

            //Act
            await _handler.Handle(request);

            //Assert
            _repository.Verify(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenTheResponseIsMappedCorrectly()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest();

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(_testRecord.Id, result.Data.Id);
            Assert.AreEqual(_testRecord.ApprenticeshipId, result.Data.ApprenticeshipId);
            Assert.AreEqual(_testRecord.Originator, (Originator) result.Data.Originator);
            Assert.AreEqual(_testRecord.FirstName, result.Data.FirstName);
            Assert.AreEqual(_testRecord.LastName, result.Data.LastName);
            Assert.AreEqual(_testRecord.DateOfBirth, result.Data.DateOfBirth);
            Assert.AreEqual(_testRecord.ULN, result.Data.ULN);
            Assert.AreEqual(_testRecord.TrainingType, (TrainingType?) result.Data.TrainingType);
            Assert.AreEqual(_testRecord.TrainingCode, result.Data.TrainingCode);
            Assert.AreEqual(_testRecord.TrainingName, result.Data.TrainingName);
            Assert.AreEqual(_testRecord.Cost, result.Data.Cost);
            Assert.AreEqual(_testRecord.StartDate, result.Data.StartDate);
            Assert.AreEqual(_testRecord.EndDate, result.Data.EndDate);
        }
    }
}
