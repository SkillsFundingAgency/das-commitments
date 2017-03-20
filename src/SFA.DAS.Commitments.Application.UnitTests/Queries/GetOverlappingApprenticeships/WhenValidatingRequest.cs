using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Validation;
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
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains("Uln")));
        }

        [Test]
        public void ThenStartDateIsRequired()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(ApprenticeshipOverlapValidationRequest.StartDate))));
        }

        [Test]
        public void ThenEndDateIsRequired()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains(nameof(ApprenticeshipOverlapValidationRequest.EndDate))));
        }
    }
}
