using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Domain.Validation;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models
{
    [TestFixture]
    public class CommitmentTests
    {
        [Test]
        public void AddDraftApprenticeship_WithNonNullDetails_ShouldCallDomainValidate()
        {
            // Arrange
            var commitment = new Commitment();
            var draftApprenticeship = new DraftApprenticeshipDetails();

            var domainValidatorMock = new Mock<IDomainValidator>();
            domainValidatorMock
                .Setup(dv => dv.Validate(draftApprenticeship))
                .Returns(new DomainError[0])
                .Verifiable("Draft apprenticeship was not validated");

            // Act
            commitment.AddDraftApprenticeship(draftApprenticeship, domainValidatorMock.Object);

            // Assert
            domainValidatorMock.VerifyAll();
        }
    }
}