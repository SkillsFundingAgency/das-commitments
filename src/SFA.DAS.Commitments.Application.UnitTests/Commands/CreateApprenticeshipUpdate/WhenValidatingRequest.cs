using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain;

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
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder()
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains(nameof(PendingApprenticeshipUpdatePlaceholder.ApprenticeshipId))));

        }

        [Test]
        public void ThenIfNoFieldsWereChangedThenIsNotValid()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder
                {
                    ApprenticeshipId = 1,
                    Originator = CallerType.Employer
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
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder
                {
                    ApprenticeshipId = 1,
                    Originator = CallerType.Employer,
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
