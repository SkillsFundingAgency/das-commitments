using System;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetEmployerAccountSummary
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetEmployerAccountSummaryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetEmployerAccountSummaryValidator();
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void ThenEmployerAccountIdMustBeValid(long employerAccountId, bool isExpectedToBeValid)
        {
            // arrange
            var caller = new Caller {CallerType = CallerType.Employer, Id = employerAccountId};
            var request = new GetEmployerAccountSummaryRequest {Caller = caller};

            // act
            var result = _validator.Validate(request);

            // assert
            Assert.AreEqual(isExpectedToBeValid, result.IsValid);

            if (!isExpectedToBeValid)
                Assert.IsTrue(result.Errors.Any(x => x.PropertyName.Contains("Id")));
        }
    }
}
