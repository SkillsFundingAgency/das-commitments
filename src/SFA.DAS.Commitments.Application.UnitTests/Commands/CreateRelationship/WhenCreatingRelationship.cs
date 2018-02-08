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
                Relationship = new Relationship {EmployerAccountId = 1, ProviderId = 2, LegalEntityId = "3"}
            };

            _validator.Setup(x => x.Validate(It.IsAny<CreateRelationshipCommand>())).Returns(new ValidationResult());

            // Act
            await _handler.Handle(request);

            // Assert
            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<RelationshipEvent>(y =>
                    y.ProviderId == request.Relationship.ProviderId &&
                    y.EmployerAccountId == request.Relationship.EmployerAccountId &&
                    y.LegalEntityId == request.Relationship.LegalEntityId)), Times.Once);
        }
    }
}
