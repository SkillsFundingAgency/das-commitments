using System;
using System.Globalization;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class UpdateDraftApprenticeshipRequestValidatorTests
    {
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_FirstName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest { FirstName = value};
            
            AssertValidationResult(r => r.FirstName, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_LastName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest {LastName = value};
            
            AssertValidationResult(r => r.LastName, request, expectedValid);
        }

        [TestCase("", true)]
        [TestCase("2001-05-01", true)]
        public void Validate_DateOfBirth_ShouldBeValidated(string value, bool expectedValid)
        {
            var dateOfBirth = string.IsNullOrWhiteSpace(value)
                ? (DateTime?) null
                : DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            
            var request = new UpdateDraftApprenticeshipRequest { DateOfBirth = dateOfBirth };

            AssertValidationResult(r => r.DateOfBirth, request,  expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX50", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_Uln_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest { Uln = value };
            
            AssertValidationResult(r => r.Uln, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_CourseCode_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest { CourseCode = value };
            
            AssertValidationResult(r => r.CourseCode, request, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(null, true)]
        public void Validate_Cost_ShouldBeValidated(int? value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest { Cost = value };
            
            AssertValidationResult(r => r.Cost, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX50", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_ProviderReference_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new UpdateDraftApprenticeshipRequest { Reference = value };
            
            AssertValidationResult(r => r.Reference, request, expectedValid);
        }

        [Test]
        public void Validate_UserInfoIsNull_ShouldBeValid()
        {
            var request = new UpdateDraftApprenticeshipRequest { UserInfo = null};
            
            AssertValidationResult(r => r.UserInfo, request, true);
        }

        [Test]
        public void Validate_UserInfoIsNotNullAndHasGoodData_ShouldBeValid()
        {
            var userInfo = new UserInfo { UserId = "EE", UserDisplayName = "Name", UserEmail = "a@a.com" };
            var request = new UpdateDraftApprenticeshipRequest { UserInfo = userInfo };

            AssertValidationResult(r => r.UserInfo, request, true);
        }

        private static void AssertValidationResult<T>(Expression<Func<UpdateDraftApprenticeshipRequest, T>> property, UpdateDraftApprenticeshipRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new UpdateDraftApprenticeshipRequestValidator();

            // Act
            var result = validator.TestValidate(request);
            
            // Assert
            if (expectedValid)
            {
                result.ShouldNotHaveValidationErrorFor(property);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(property);
            }
        }
    }
}
