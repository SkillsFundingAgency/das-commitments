using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.TriageDataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.TriageDataLock
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private TriageDataLockCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new TriageDataLockCommandValidator();
        }

        [Test]
        public void ThenApprenticeshipIdMustBeSpecified()
        {
            //Arrange
            var command = new TriageDataLockCommand();

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(TriageDataLockCommand.ApprenticeshipId))));
        }

        [Test]
        public void ThenDataLockEventIdMustBeSpecified()
        {
            //Arrange
            var command = new TriageDataLockCommand();

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(TriageDataLockCommand.DataLockEventId))));
        }
    }
}
