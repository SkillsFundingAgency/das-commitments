using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateDataLock
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private UpdateDataLockTriageStatusCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new UpdateDataLockTriageStatusCommandValidator();
        }

        [Test]
        public void ThenApprenticeshipIdMustBeSpecified()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand();

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(UpdateDataLockTriageStatusCommand.ApprenticeshipId))));
        }

        [Test]
        public void ThenDataLockEventIdMustBeSpecified()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand();

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(UpdateDataLockTriageStatusCommand.DataLockEventId))));
        }
    }
}
