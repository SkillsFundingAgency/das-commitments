using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfPartyRequest
{
    [TestFixture]
    public class GetChangeOfPartyRequestQueryValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void ValidateChangeOfPartyRequestId(long changeOfPartyRequestId, bool expectedIsValid)
        {
            var validator = new GetChangeOfPartyRequestQueryValidator();

            var result = validator.Validate(new GetChangeOfPartyRequestQuery(changeOfPartyRequestId));

            Assert.AreEqual(expectedIsValid, result.IsValid);
        }
    }
}
