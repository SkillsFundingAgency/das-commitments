using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetPendingApprenticeshipUpdate
{
    [TestFixture()]
    public class WhenGettingPendingApprenticeshipUpdate
    {
        private Mock<GetPendingApprenticeshipUpdateValidator> _validator;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        private ApprenticeshipUpdate _testRecord;

        private GetPendingApprenticeshipUpdateQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<GetPendingApprenticeshipUpdateValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<GetPendingApprenticeshipUpdateRequest>()))
                .Returns(() => new ValidationResult());

            var testApprenticeship = new Apprenticeship
            {
                Id = 999,
                EmployerAccountId = 888,
                ProviderId = 777
            };

            _testRecord = new ApprenticeshipUpdate
            {
                Id = 1,
                ApprenticeshipId = 1,
                Originator = Originator.Employer,
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(2000, 01, 03),
                TrainingType = TrainingType.Framework,
                TrainingCode = "AB-123",
                TrainingName = "Test Course",
                Cost = 1000,
                StartDate = new DateTime(2018, 1, 1),
                EndDate = new DateTime(2018, 6, 1)
            };

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(testApprenticeship);

            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(_testRecord);

            _handler = new GetPendingApprenticeshipUpdateQueryHandler(_validator.Object, _apprenticeshipUpdateRepository.Object, _apprenticeshipRepository.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller { CallerType = CallerType.Employer, Id = 888 }
            };

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
        public void ThenIfTheCallerIdIsNotAuthorizedThenExceptionIsThrown()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller { CallerType = CallerType.Employer, Id = 123 }
            };
           
            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveData()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller {CallerType = CallerType.Employer, Id = 888}
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenTheResponseIsMappedCorrectly()
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller {CallerType = CallerType.Employer, Id = 888}
            };

            //Act.
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(_testRecord.Id, result.Data.Id);
            Assert.AreEqual(_testRecord.ApprenticeshipId, result.Data.ApprenticeshipId);
            Assert.AreEqual(_testRecord.Originator, (Originator) result.Data.Originator);
            Assert.AreEqual(_testRecord.FirstName, result.Data.FirstName);
            Assert.AreEqual(_testRecord.LastName, result.Data.LastName);
            Assert.AreEqual(_testRecord.DateOfBirth, result.Data.DateOfBirth);
            Assert.AreEqual(_testRecord.TrainingType, (TrainingType?) result.Data.TrainingType);
            Assert.AreEqual(_testRecord.TrainingCode, result.Data.TrainingCode);
            Assert.AreEqual(_testRecord.TrainingName, result.Data.TrainingName);
            Assert.AreEqual(_testRecord.Cost, result.Data.Cost);
            Assert.AreEqual(_testRecord.StartDate, result.Data.StartDate);
            Assert.AreEqual(_testRecord.EndDate, result.Data.EndDate);
        }
    }
}
