using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprentice
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

            // assert
            if (expectToBeValid)
            {
                validator.ShouldNotHaveValidationErrorFor(r => r.EmployerAccountId, request, null);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(r => r.EmployerAccountId, request, null);
            }
        }
    }
}