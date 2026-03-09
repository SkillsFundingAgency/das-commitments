using System.Linq.Expressions;
using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class CocApprovalRequestValidatorTests
    {
        [Test]
        public void Validate_LearningKey_ShouldNotBeZero()
        {
            var request = new CocApprovalRequest();
            
            AssertValidationResult(r => r.LearningKey, request, false);
        }

        [Test]
        public void Validate_ApprenticeshipId_ShouldNotBeZero()
        {
            var request = new CocApprovalRequest();

            AssertValidationResult(r => r.ApprenticeshipId, request, false);
        }

        [Test]
        public void Validate_UKPRN_ShouldNotBeLongerThan10Chars()
        {
            var request = new CocApprovalRequest { UKPRN = "12345678901" };

            AssertValidationResult(r => r.UKPRN, request, false);
        }

        [Test]
        public void Validate_ULN_ShouldNotBeLongerThan10Chars()
        {
            var request = new CocApprovalRequest { ULN = "12345678901" };

            AssertValidationResult(r => r.ULN, request, false);
        }

        [TestCase("Apprenticeship", true)]
        [TestCase("FoundationApprenticeship", true)]
        [TestCase("ApprenticeshipUnit", true)]
        [TestCase("XXXXXX", false)]

        public void Validate_LearningType_ShouldBeInEnumList(string learningType, bool expected)
        {
            var request = new CocApprovalRequest { LearningType = learningType };

            AssertValidationResult(r => r.LearningType, request, expected);
        }

        [TestCase("TNP1", true)]
        [TestCase("TNP2", true)]
        [TestCase("XXXXXX", false)]
        public void Validate_ChangeType_ShouldBeInEnumList(string changeType, bool expected)
        {
            var changes = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange { ChangeType = changeType, Data = new CocData { New = "NewValue", Old = "OldValue" } }
            };
            var request = new CocApprovalRequest { Changes = changes };

            AssertValidationResult(r => r.Changes, request, expected);
        }

        [Test]
        public void Validate_ChangeType_ShouldNotBeDuplicated()
        {
            var changes = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange { ChangeType = "TNP1", Data = new CocData { New = "NewValue", Old = "OldValue" } },
                new CocApprovalFieldChange { ChangeType = "TNP1", Data = new CocData { New = "NewValue2", Old = "OldValue2" } }
            };
            var request = new CocApprovalRequest { Changes = changes };

            AssertValidationResult(r => r.Changes, request, false);
        }

        [Test]
        public void Validate_ChangeType_ShouldNotAllowDataToBeNull()
        {
            var changes = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange { ChangeType = "TNP1", Data = null },
                new CocApprovalFieldChange { ChangeType = "TNP2", Data = new CocData { New = "NewValue2", Old = "OldValue2" } }
            };
            var request = new CocApprovalRequest { Changes = changes };

            AssertValidationResult(r => r.Changes, request, false);
        }

        [Test]
        public void Validate_ChangeType_ShouldNotAllowDataNewAndOldToBeIdentical()
        {
            var changes = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange { ChangeType = "TNP1", Data = null },
                new CocApprovalFieldChange { ChangeType = "TNP2", Data = new CocData { New = "Value2", Old = "Value2" } }
            };
            var request = new CocApprovalRequest { Changes = changes };

            AssertValidationResult(r => r.Changes, request, false);
        }

        [Test]
        public void Validate_Changes_ShouldNotBeEmpty()
        {
            var request = new CocApprovalRequest { Changes = new List<CocApprovalFieldChange>() };

            AssertValidationResult(r => r.Changes, request, false);
        }

        private static void AssertValidationResult<T>(Expression<Func<CocApprovalRequest, T>> property, CocApprovalRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new CocApprovalRequestValidator();

            // Act
            var result = validator.TestValidate(request);
            
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
