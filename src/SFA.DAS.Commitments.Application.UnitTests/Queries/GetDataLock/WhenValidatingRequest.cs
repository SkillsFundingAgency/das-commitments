using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetDataLock
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetDataLockValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetDataLockValidator();
        }

        [Test]
        public void ThenDataLockEventIdMustBeSpecified()
        {
            //Arrange
            var request = new GetDataLockRequest();

            //Act
            var result = _validator.Validate(request);

            //Arrange
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(
                nameof(GetDataLockRequest.DataLockEventId))));
        }

        [Test]
        public void ThenApprenticeshipIdMustBeSpecified()
        {
            //Arrange
            var request = new GetDataLockRequest();

            //Act
            var result = _validator.Validate(request);

            //Arrange
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(
                nameof(GetDataLockRequest.ApprenticeshipId))));
        }
    }
}
