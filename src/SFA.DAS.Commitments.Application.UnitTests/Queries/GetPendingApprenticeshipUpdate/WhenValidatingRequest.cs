using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetPendingApprenticeshipUpdate
{
    [TestFixture()]
    public class WhenValidatingRequest
    {
        private GetPendingApprenticeshipUpdateValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetPendingApprenticeshipUpdateValidator();
        }

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        [TestCase(9999, true)]
        public void ThenIfApprenticeshipIdIsGreaterThanZeroThenIsValid(long id, bool isValid)
        {
            //Arrange
            var request = new GetPendingApprenticeshipUpdateRequest
            {
                ApprenticeshipId = id
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.AreEqual(isValid, result.IsValid);
        }
    }
}
