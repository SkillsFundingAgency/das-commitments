using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprovedApprenticeship
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetApprovedApprenticeshipRequestValidator _validator;
        private GetApprovedApprenticeshipRequest _validRequest;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetApprovedApprenticeshipRequestValidator();
            _validRequest = new GetApprovedApprenticeshipRequest
            {
                ApprenticeshipId = 1,
                Caller = new Caller(2, CallerType.Employer)
            };
        }

        [Test]
        public void ThenIsValidIfAllRequiredPropertiesAreSpecified()
        {
            var result = _validator.Validate(_validRequest);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ThenApprenticeshipIdMustBeSpecified()
        {
            _validRequest.ApprenticeshipId = 0;

            var result = _validator.Validate(_validRequest);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ThenCallerMustBeSpecified()
        {
            _validRequest.Caller = null;

            var result = _validator.Validate(_validRequest);

            Assert.IsFalse(result.IsValid);
        }


        [Test]
        public void ThenCallerIdMustBeSpecified()
        {
            _validRequest.Caller.Id = 0;

            var result = _validator.Validate(_validRequest);

            Assert.IsFalse(result.IsValid);
        }
    }
}
