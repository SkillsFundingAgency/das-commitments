using System.Threading.Tasks;
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
        public async Task AddDraftApprenticeship_WithNonNullDetails_ShouldCallDomainValidate()
        {
            // Arrange
            var commitment = new Commitment();
            var draftApprenticeship = new DraftApprenticeshipDetails();

            var domainValidatorMock = new Mock<IDomainValidator>();
            domainValidatorMock
                .Setup(dv => dv.ValidateAsync(draftApprenticeship))
                .ReturnsAsync(new DomainError[0])
                .Verifiable("Draft apprenticeship was not validated");

            // Act
            await commitment.AddDraftApprenticeshipAsync(draftApprenticeship, domainValidatorMock.Object);

            // Assert
            domainValidatorMock.VerifyAll();
        }
    }
}