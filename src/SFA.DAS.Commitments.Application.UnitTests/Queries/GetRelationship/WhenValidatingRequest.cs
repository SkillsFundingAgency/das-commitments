using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetRelationship
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetRelationshipValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetRelationshipValidator();
        }

        [Test]
        public void ThenRequestIsValidIfAllPropertiesAreProvided()
        {
            // Arrange
            var request = new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ThenEmployerAccountIdIsMandatory()
        {
            //Arrange
            var request = new GetRelationshipRequest
            {
                EmployerAccountId = 0,
                ProviderId = 2,
                LegalEntityId = "L3"
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x=> x.PropertyName =="EmployerAccountId"));
        }

        [Test]
        public void ThenProviderIdisMandatory()
        {
            //Arrange
            var request = new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 0,
                LegalEntityId = "L3"
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "ProviderId"));
        }

        [Test]
        public void ThenLegalEntityIsMandatory()
        {
            //Arrange
            var request = new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = ""
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "LegalEntityId"));
        }
    }
}
