using System.Linq.Expressions;
using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Validation;
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

        [TestCase(" ", false)]
        [TestCase("9999999999", false)]
        [TestCase("1234567890", true)]
        public void Validate_ULN_ShouldUseExistingUlnFormatValidator(string uln, bool expectedValid)
        {
            var request = CreateValidRequest();
            request.ULN = uln;

            AssertValidationResult(
                r => r.ULN,
                request,
                expectedValid,
                mock =>
                {
                    mock.Setup(x => x.Validate(" ")).Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);
                    mock.Setup(x => x.Validate("9999999999")).Returns(UlnValidationResult.IsInvalidUln);
                    mock.Setup(x => x.Validate("1234567890")).Returns(UlnValidationResult.Success);
                });
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

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("/ABCxyz-123", true)]
        [TestCase("ABCxyz-123", true)]
        [TestCase("///aB9-", true)]
        [TestCase(" ", false)]
        [TestCase("https://example.com/path", false)]
        [TestCase("abc_def", false)]
        [TestCase("abc.def", false)]
        [TestCase("abc?x=1", false)]
        [TestCase("<script>alert(1)</script>", false)]
        [TestCase("javascript:void(0)", false)]
        public void Validate_ApprovedUri_ShouldMatchApprovedCharacterAllowList(string approvedUri, bool expectedValid)
        {
            var request = CreateValidRequest();
            request.ApprovedUri = approvedUri;

            AssertValidationResult(r => r.ApprovedUri, request, expectedValid);
        }

        [Test]
        public void Validate_ApprovedUri_ShouldRejectWhenLengthExceedsMaximum()
        {
            var tooLong = "https://" + new string('a', ApprovedUriValidation.MaxLength + 1);
            var request = CreateValidRequest();
            request.ApprovedUri = tooLong;

            AssertValidationResult(r => r.ApprovedUri, request, false);
        }

        private static CocApprovalRequest CreateValidRequest()
        {
            return new CocApprovalRequest
            {
                LearningKey = Guid.NewGuid(),
                ApprenticeshipId = 1,
                UKPRN = "1234567890",
                ULN = "1234567890",
                LearningType = "Apprenticeship",
                Changes =
                [
                    new CocApprovalFieldChange { ChangeType = "TNP1", Data = new CocData { New = "1", Old = "2" } }
                ]
            };
        }

        private static void AssertValidationResult<T>(
            Expression<Func<CocApprovalRequest, T>> property,
            CocApprovalRequest request,
            bool expectedValid,
            Action<Mock<IUlnValidator>> setup = null)
        {
            // Arrange
            var ulnValidator = new Mock<IUlnValidator>();
            ulnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.Success);
            setup?.Invoke(ulnValidator);
            var validator = new CocApprovalRequestValidator(ulnValidator.Object);

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
