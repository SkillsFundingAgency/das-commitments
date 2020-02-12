using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    [TestFixture]
    public class GetApprenticeshipsValidationTests
    {
        [TestCase(0, 1, true)]
        [TestCase(1, 0, true)]
        [TestCase(null, 1, true)]
        [TestCase(1, null, true)]
        [TestCase(1, 1, false)]
        [TestCase(0, 0, false)]
        [TestCase(null, null, false)]
        [TestCase(null, 0, false)]
        [TestCase(0, null, false)]
        public void Validate_WithSpecifiedProviderAndEmployerId_ShouldSetIsValidCorrectly(long? providerId, long? employerId, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsQuery
            {
                EmployerAccountId = employerId,
                ProviderId = providerId,
                PageNumber = 1,
                PageItemCount = 1
            });

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [TestCase("test",false)]
        [TestCase(null,true)]
        [TestCase("",true)]
        [TestCase(nameof(Apprenticeship.FirstName),true)]
        [TestCase(nameof(Apprenticeship.Cohort.LegalEntityName),true)]
        public void Validate_WithSpecifiedSortField_ShouldOnlyBeAllowedIfPropertyOnApprenticeship(string fieldName, bool expected)
        {
            // arrange
            var validator = new GetApprenticeshipsQueryValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsQuery
            {
                ProviderId = 1, 
                SortField = fieldName,
                PageNumber = 1,
                PageItemCount = 1
            });

            // act
            var actual = validationResults.IsValid;

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}
