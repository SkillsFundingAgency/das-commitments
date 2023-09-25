using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProviderPaymentsPriority
{
    [TestFixture]
    [Parallelizable]
    public class GetProviderPaymentsPriorityQueryValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WhenValidatingEmployerAccountId_ThenShouldRejectNonPositiveNumbers(long accountId, bool expectToBeValid)
        {
            // arrange
            var validator = new GetProviderPaymentsPriorityQueryValidator();
            
            // act
            var request = new GetProviderPaymentsPriorityQuery(accountId);
            var result = validator.TestValidate(request);

            // assert
            if (expectToBeValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.EmployerAccountId);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.EmployerAccountId);
            }
        }
    }
}