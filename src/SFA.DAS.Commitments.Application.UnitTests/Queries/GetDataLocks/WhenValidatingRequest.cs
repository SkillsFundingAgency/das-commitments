using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetDataLocks
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetDataLocksValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetDataLocksValidator();
        }

        [Test]
        public void ThenApprenticeshipIdMustBeSpecified()
        {
            //Arrange
            var request = new GetDataLocksRequest();

            //Act
            var result = _validator.Validate(request);

            //Arrange
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(
                nameof(GetDataLocksRequest.ApprenticeshipId))));
        }
    }
}
