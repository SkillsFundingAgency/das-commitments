using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetOverlappingEmailApprenticeships
{
    [TestFixture]
    public class WhenValidatingRequest
    {

        private GetEmailOverlappingApprenticeshipsValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetEmailOverlappingApprenticeshipsValidator();
        }

        [Test]
        public void ThenTheRequestMustContainAtLeastOneRecord()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenEmailsAreRequired()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                    new ApprenticeshipEmailOverlapValidationRequest()
                }
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains("Email")));
        }

        [Test]
        public void ThenStartDateIsRequired()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                    new ApprenticeshipEmailOverlapValidationRequest()
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
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                    new ApprenticeshipEmailOverlapValidationRequest()
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