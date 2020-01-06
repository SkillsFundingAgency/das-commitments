using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    [TestFixture]
    public class GetApprenticeshipsValidationTests
    {
        [TestCase( (uint) 0, false)]
        [TestCase( (uint) 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(uint id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsRequest
            {
                ProviderId = id,
                PageNumber = 1,
                PageItemCount = 1
            });

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithPageNumber_ShouldSetIsValidCorrectly(int pageNumber, bool expectedIsValid)
        {
            //Arrange
            var validator = new GetApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsRequest
            {
                ProviderId = 1, 
                PageNumber = pageNumber, 
                PageItemCount = 1
            });

            //Act
            var actualIsValid = validationResults.IsValid;

            //Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WithPageItemCount_ShouldSetIsValidCorrectly(int itemCount, bool expectedIsValid)
        {
            // arrange
            var validator = new GetApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsRequest
            {
                ProviderId = 1, 
                PageNumber = 1, 
                PageItemCount = itemCount
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
            var validator = new GetApprenticeshipsValidator();
            var validationResults = validator.Validate(new GetApprenticeshipsRequest
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
