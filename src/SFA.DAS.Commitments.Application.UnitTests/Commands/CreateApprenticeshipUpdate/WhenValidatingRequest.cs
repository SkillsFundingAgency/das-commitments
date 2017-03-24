using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeshipUpdate
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private CreateApprenticeshipUpdateValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateApprenticeshipUpdateValidator();
        }

        [Test]
        public void ThenApprenticeshipIdIsMandatory()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains(nameof(ApprenticeshipUpdate.ApprenticeshipId))));

        }

        [Test]
        public void ThenIfNoFieldsWereChangedThenIsNotValid()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Employer
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenIfOneFieldWasChangedThenIsValid()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
                {
                    ApprenticeshipId = 1,
                    Originator = Originator.Employer,
                    FirstName = "Test"
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsTrue(result.IsValid);
        }
    }
}
