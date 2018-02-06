using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetActiveApprenticeshipsByUln
{
    [TestFixture]
    public class WhenValidatingGetActiveApprenticeshipsByUlnRequest
    {
        private GetActiveApprenticeshipsByUlnValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetActiveApprenticeshipsByUlnValidator();
        }

        [Test]
        public void ThenTheRequestMustContainAtLeastOneRecord()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest();

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenUlnsAreRequired()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest
            {
               Uln = ""
            };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName.Contains("Uln")));
        }


        [Test]
        public void ShouldReturnTrueWhenRequestIsValid()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest
            {
                Uln = "5830206233"
            };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }
    }
}
