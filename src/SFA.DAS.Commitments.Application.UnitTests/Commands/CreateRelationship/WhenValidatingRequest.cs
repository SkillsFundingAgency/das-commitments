using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateRelationship
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private CreateRelationshipValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateRelationshipValidator();
        }

        [Test]
        public void ThenRequestIsValidIfAllPropertiesAreProvided()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ThenEmployerAccountIdIsMandatory()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();
            request.Relationship.EmployerAccountId = 0;

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e=> e.PropertyName == "Relationship.EmployerAccountId"));
        }

        [Test]
        public void ThenProviderIdIsMandatory()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();
            request.Relationship.ProviderId = 0;

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Relationship.ProviderId"));
        }

        [Test]
        public void ThenProviderNameIsMandatory()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();
            request.Relationship.ProviderName = string.Empty;

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Relationship.ProviderName"));
        }

        [Test]
        public void ThenLegalEntityIdIsMandatory()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();
            request.Relationship.LegalEntityId = string.Empty;

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Relationship.LegalEntityId"));
        }

        [Test]
        public void ThenLegalEntityNameIsMandatory()
        {
            // Arrange
            var request = GetCompleteCreateRelationshipCommand();
            request.Relationship.LegalEntityName = string.Empty;

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Relationship.LegalEntityName"));
        }

        private CreateRelationshipCommand GetCompleteCreateRelationshipCommand()
        {
            return new CreateRelationshipCommand
            {
                Relationship = new Domain.Entities.Relationship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ProviderName = "Provider",
                    LegalEntityId = "L3",
                    LegalEntityName = "Legal Entity"
                }
            };
        }

    }

}




