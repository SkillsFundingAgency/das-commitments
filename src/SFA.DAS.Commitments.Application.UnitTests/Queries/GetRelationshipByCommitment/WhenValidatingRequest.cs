using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetRelationshipByCommitment
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetRelationshipByCommitmentValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetRelationshipByCommitmentValidator();
        }

        [Test]
        public void ThenRequestIsValidIfAllPropertiesAreProvided()
        {
            // Arrange
            var request = new GetRelationshipByCommitmentRequest
            {
                ProviderId = 1,
                CommitmentId = 2
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ThenProviderIdIsMandatory()
        {
            //Arrange
            var request = new GetRelationshipByCommitmentRequest
            {
                ProviderId = 0,
                CommitmentId = 2
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "ProviderId"));
        }

        [Test]
        public void ThenCommitmentIdIsMandatory()
        {
            //Arrange
            var request = new GetRelationshipByCommitmentRequest
            {
                ProviderId = 1,
                CommitmentId = 0
            };

            //Act
            var result = _validator.Validate(request);

            //Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "CommitmentId"));
        }
    }
}
