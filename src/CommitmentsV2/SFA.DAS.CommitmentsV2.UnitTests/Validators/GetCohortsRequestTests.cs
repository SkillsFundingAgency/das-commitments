using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class GetCohortsRequestTests
    {
        [TestCase(1, null, true)]
        [TestCase(null, null, false)]
        [TestCase(null, 1, true)]
        [TestCase(1, 1, true)]
        public void Validate_AccountIdAndProviderId_ShouldBeValidated(long? accountId, long? providerId, bool expectedValid)
        {
            var request = new GetCohortsRequest { AccountId = accountId, ProviderId = providerId };
            var validator = new GetCohortsRequestValidator();

            var result = validator.TestValidate(request);
            Assert.That(result.IsValid, Is.EqualTo(expectedValid));
        }
    }
}