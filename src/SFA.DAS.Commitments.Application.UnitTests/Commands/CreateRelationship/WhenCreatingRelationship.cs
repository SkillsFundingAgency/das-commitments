using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateRelationship
{
    [TestFixture]
    public class WhenCreatingRelationship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<AbstractValidator<CreateRelationshipCommand>> _validator;
        private CreateRelationshipCommandHandler _handler;
        private Mock<IMessagePublisher> _messagePublisher;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<CreateRelationshipCommand>>();
           
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _mockCommitmentRespository.Setup(
                x => x.CreateRelationship(It.IsAny<Domain.Entities.Relationship>()))
                    .ReturnsAsync(1);

            _handler = new CreateRelationshipCommandHandler(_mockCommitmentRespository.Object,
                _validator.Object, Mock.Of<ICommitmentsLogger>(), _messagePublisher.Object);
        }

        [Test]
        public async Task ThenIfRequestIsValidThenTheRepositoryIsCalled()
        {
            //Arrange
            _validator.Setup(x => x.Validate(It.IsAny<CreateRelationshipCommand>()))
                .Returns(() => new ValidationResult());

            //Act
            await _handler.Handle(new CreateRelationshipCommand
            {
                Relationship = new Domain.Entities.Relationship()
            });

            //Assert
            _mockCommitmentRespository.Verify(
                x => x.CreateRelationship(It.IsAny<Domain.Entities.Relationship>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRelationshipEventIsCreated()
        {
            // Arrange
            var request = new CreateRelationshipCommand
            {
                Relationship = new Relationship
                {
                    EmployerAccountId = 1,
                    LegalEntityId = "123",
                    LegalEntityName = "Name",
                    LegalEntityAddress = "Street 1, Street 2, Town, City, Postcode",
                    LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                    ProviderId = 2,
                    ProviderName = "Provider name",
                    Verified = false
                }
            };

            _validator.Setup(x => x.Validate(It.IsAny<CreateRelationshipCommand>())).Returns(new ValidationResult());

            // Act
            await _handler.Handle(request);

            // Assert
            _messagePublisher.Verify(x => x.PublishAsync(It.Is<RelationshipCreated>(y =>
                y.Relationship.EmployerAccountId == request.Relationship.EmployerAccountId &&
                y.Relationship.LegalEntityId == request.Relationship.LegalEntityId &&
                y.Relationship.LegalEntityName == request.Relationship.LegalEntityName &&
                y.Relationship.LegalEntityAddress == request.Relationship.LegalEntityAddress &&
                y.Relationship.LegalEntityOrganisationType == request.Relationship.LegalEntityOrganisationType &&
                y.Relationship.ProviderId == request.Relationship.ProviderId &&
                y.Relationship.ProviderName == request.Relationship.ProviderName &&
                y.Relationship.Verified == request.Relationship.Verified
            )), Times.Once);
        }
    }
}
