using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Queries.GetEmployer;

namespace SFA.DAS.CommitmentsV2.UnitTests.Queries.GetEmployer
{
    [TestFixture]
    public class GetEmployerValidationTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, true)]
        [TestCase( 1, true)]
        public void Validate_WithSpecifiedId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetEmployerValidator();
            var validationResults = validator.Validate(new GetEmployerRequest {AccountLegalEntityId = id});

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}
