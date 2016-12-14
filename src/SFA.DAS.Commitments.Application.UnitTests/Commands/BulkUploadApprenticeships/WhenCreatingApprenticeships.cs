using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeships
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private BulkUploadApprenticeshipsCommandHandler _handler;
        private BulkUploadApprenticeshipsCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new BulkUploadApprenticeshipsCommandHandler(_mockCommitmentRespository.Object, new BulkUploadApprenticeshipsValidator(), _mockApprenticeshipEvents.Object, Mock.Of<ICommitmentsLogger>());

            var exampleApprenticships = new List<Apprenticeship>
            {
                new Apprenticeship { FirstName = "Bob", LastName = "Smith", ULN = "1234567890" },
                new Apprenticeship { FirstName = "Jane", LastName = "Jones", ULN = "1122334455" },
            };

            _exampleValidRequest = new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeships = exampleApprenticships
            };

            var existingCommitment = new Domain.Entities.Commitment { ProviderId = 111L, EditStatus = Domain.Entities.EditStatus.ProviderOnly };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);
        }

        [Test]
        public async Task ShouldCallCommitmentRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.CreateApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Domain.Entities.Apprenticeship>>()), Times.Once);
        }

        [Test]
        public void ShouldThrowExceptionIfEditStatusIncorrect()
        {
            var existingCommitment = new Domain.Entities.Commitment
            {
                ProviderId = 111L,
                EditStatus = Domain.Entities.EditStatus.EmployerOnly // Expecting ProviderOnly
            };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentInIncorrectState()
        {
            var existingCommitment = new Domain.Entities.Commitment
            {
                ProviderId = 111L,
                EditStatus = Domain.Entities.EditStatus.ProviderOnly,
                CommitmentStatus = Domain.Entities.CommitmentStatus.Deleted // Expecting Active or New
            }; 

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerIdIsNotOwner()
        {
            var existingCommitment = new Domain.Entities.Commitment
            {
                ProviderId = 999L, // Expecting 111L
                EditStatus = Domain.Entities.EditStatus.ProviderOnly,
                CommitmentStatus = Domain.Entities.CommitmentStatus.Active 
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentDoesNotExist()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(null);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ResourceNotFoundException>();
        }
    }
}
