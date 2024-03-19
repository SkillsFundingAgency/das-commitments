using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    [TestFixture]
    public class GetApprenticeshipsValidationTests
    {
        [TestCase("test",false)]
        [TestCase(null,true)]
        [TestCase("",true)]
        [TestCase(nameof(Apprenticeship.FirstName),true)]
        [TestCase(nameof(Apprenticeship.Cohort.AccountLegalEntity.Name),true)]
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
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
