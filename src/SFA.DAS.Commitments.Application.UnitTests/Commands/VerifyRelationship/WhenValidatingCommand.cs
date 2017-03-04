using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.VerifyRelationship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.VerifyRelationship
{   
    [TestFixture]
    public class WhenValidatingCommand
    {
        private VerifyRelationshipValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new VerifyRelationshipValidator();
        }

        [Test]
        public void ThenRequestIsValidIfAllFieldsAreProvided()
        {
            //Arrange
            var request = new VerifyRelationshipCommand
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3",
                UserId = "User",
                Verified = true
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
            var request = new VerifyRelationshipCommand();
            
            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e=> e.PropertyName=="EmployerAccountId"));
        }

        [Test]
        public void ThenLegalEntityIdIsMandatory()
        {
            //Arrange
            var request = new VerifyRelationshipCommand();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "LegalEntityId"));
        }

        [Test]
        public void ThenProviderIdIsMandatory()
        {
            //Arrange
            var request = new VerifyRelationshipCommand();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "ProviderId"));
        }

        [Test]
        public void ThenUserIdIsMandatory()
        {
            //Arrange
            var request = new VerifyRelationshipCommand();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "UserId"));
        }

        [Test]
        public void ThenVerifiedIsMandatory()
        {
            //Arrange
            var request = new VerifyRelationshipCommand();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Verified"));
        }
    }
}
