using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetRelationship
{
    [TestFixture]
    public class WhenGettingRelationship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<AbstractValidator<GetRelationshipRequest>> _validator;
        private Relationship _repositoryRecord;
        private GetRelationshipQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();

            _repositoryRecord = new Relationship
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            };

            _mockCommitmentRespository.Setup(
                x => x.GetRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(_repositoryRecord);

            _validator = new Mock<AbstractValidator<GetRelationshipRequest>>();
            _validator.Setup(x => x.Validate(It.IsAny<GetRelationshipRequest>())).Returns(() => new ValidationResult());

            _handler = new GetRelationshipQueryHandler(_mockCommitmentRespository.Object, _validator.Object);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetRelationship()
        {
            //Act
            await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            });

            //Assert
            _mockCommitmentRespository.Verify(x=> x.GetRelationship(
                It.Is<long>(accountId => accountId == 1),
                It.Is<long>(providerId => providerId == 2),
                It.Is<string>(legalEntityId => legalEntityId == "L3")));
        }

        [Test]
        public async Task ThenValidationIsPerformed()
        {
            //Act
            await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            });

            //Assert
            _validator.Verify(x=> x.Validate(It.IsAny<GetRelationshipRequest>()));
        }
    }
}
