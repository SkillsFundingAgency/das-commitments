using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetOverlappingApprenticeships
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetOverlappingApprenticeshipsValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetOverlappingApprenticeshipsValidator();
        }

        [Test]
        public void ThenTheRequestMustContainAtLeastOneRecord()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenUlnsAreRequired()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                    new OverlappingApprenticeshipRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains("Uln")));
        }

        [Test]
        public void ThenDateFromIsRequired()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                    new OverlappingApprenticeshipRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains("DateFrom")));
        }

        [Test]
        public void ThenDateToIsRequired()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                    new OverlappingApprenticeshipRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains("DateTo")));
        }
    }
}
