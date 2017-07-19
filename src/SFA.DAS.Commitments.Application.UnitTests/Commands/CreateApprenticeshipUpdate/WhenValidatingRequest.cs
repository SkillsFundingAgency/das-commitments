using System.Linq;

using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeshipUpdate
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private CreateApprenticeshipUpdateValidator _validator;
        private CreateApprenticeshipUpdateCommand _command;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateApprenticeshipUpdateValidator();

            _command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Provider
                }
            };
        }

        [Test]
        public void ThenApprenticeshipIdIsMandatory()
        {
            //Arrange
            _command.ApprenticeshipUpdate = new ApprenticeshipUpdate();

            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains(nameof(ApprenticeshipUpdate.ApprenticeshipId))));

        }

        [Test]
        public void ThenIfNoFieldsWereChangedThenIsNotValid()
        {
            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenIfOneFieldWasChangedThenIsValid()
        {
            //Arrange
            _command.ApprenticeshipUpdate.FirstName = "Test";

            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ThenTheEmployerCannotModifyTheUln()
        {
            //Arrange
            _command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, CallerType.Employer),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Employer,
                    ULN = "123"
                }
            };

            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenTheEmployerCannotModifyTheProviderRef()
        {
            //Arrange
            _command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, CallerType.Employer),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Employer,
                    ProviderRef = "123"
                }
            };

            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenTheProviderCannotModifyTheEmployerRef()
        {
            //Arrange
            _command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Provider,
                    EmployerRef = "123"
                }
            };

            //Act
            var result = _validator.Validate(_command);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestCase(CallerType.Employer, Originator.Employer, true)]
        [TestCase(CallerType.Provider, Originator.Provider, true)]
        [TestCase(CallerType.Employer, Originator.Provider, false)]
        [TestCase(CallerType.Provider, Originator.Employer, false)]
        public void TheOriginatorAndTheCallerMustBeEquivalent(CallerType callerType, Originator originator, bool isValid)
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, callerType),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = originator,
                    FirstName = "Test"
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.AreEqual(isValid, result.IsValid);
        }
    }
}
