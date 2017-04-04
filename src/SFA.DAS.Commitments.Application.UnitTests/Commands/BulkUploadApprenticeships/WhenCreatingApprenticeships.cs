using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeships
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private BulkUploadApprenticeshipsCommandHandler _handler;
        private BulkUploadApprenticeshipsCommand _exampleValidRequest;
        private List<Apprenticeship> _exampleApprenticships;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IMediator> _mockMediator;
        private Commitment _existingCommitment;
        private List<Domain.Entities.Apprenticeship> _existingApprenticeships;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockMediator = new Mock<IMediator>();

            var validator = new BulkUploadApprenticeshipsValidator(new ApprenticeshipValidator(new StubCurrentDateTime()));

            _handler = new BulkUploadApprenticeshipsCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                validator,
                _mockApprenticeshipEvents.Object,
                Mock.Of<ICommitmentsLogger>(), _mockMediator.Object);

            _exampleApprenticships = new List<Apprenticeship>
            {
                new Apprenticeship { FirstName = "Bob", LastName = "Smith", ULN = "1234567890", StartDate = new DateTime(2018,5,1), EndDate = new DateTime(2018,5,2)},
                new Apprenticeship { FirstName = "Jane", LastName = "Jones", ULN = "1122334455", StartDate = new DateTime(2019,3,1), EndDate = new DateTime(2019,9,2)},
            };

            _exampleValidRequest = new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeships = _exampleApprenticships
            };

            _existingApprenticeships = new List<Domain.Entities.Apprenticeship>();
            _existingCommitment = new Commitment { ProviderId = 111L, EditStatus = EditStatus.ProviderOnly, Apprenticeships = _existingApprenticeships };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(_existingCommitment);

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse {Data = new List<OverlappingApprenticeship>()});
        }

        [Test]
        public async Task ShouldCallCommitmentRepository()
        {
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Domain.Entities.Apprenticeship>>(), It.IsAny<CallerType>(), It.IsAny<string>())).ReturnsAsync(new List<Domain.Entities.Apprenticeship>());

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.BulkUploadApprenticeships(
                It.IsAny<long>(), It.IsAny<IEnumerable<Domain.Entities.Apprenticeship>>(), It.IsAny<CallerType>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ShouldPublishApprenticeshipDeletedEvents()
        {
            var insertedApprenticeships = new List<Domain.Entities.Apprenticeship> { new Domain.Entities.Apprenticeship() };
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Domain.Entities.Apprenticeship>>(), It.IsAny<CallerType>(), It.IsAny<string>())).ReturnsAsync(insertedApprenticeships);

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipEvents.Verify(x => x.BulkPublishDeletionEvent(_existingCommitment, _existingApprenticeships, "APPRENTICESHIP-DELETED"), Times.Once);
        }

        [Test]
        public async Task ShouldPublishApprenticeshipCreatedEvents()
        {
            var insertedApprenticeships = new List<Domain.Entities.Apprenticeship> { new Domain.Entities.Apprenticeship() };
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Domain.Entities.Apprenticeship>>(), It.IsAny<CallerType>(), It.IsAny<string>())).ReturnsAsync(insertedApprenticeships);
            
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipEvents.Verify(x => x.BulkPublishEvent(_existingCommitment, insertedApprenticeships, "APPRENTICESHIP-CREATED"), Times.Once);
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

        [Test]
        public async Task ThenOverlappingApprenticeshipValidationShouldBePerformed()
        {
            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.Is<GetOverlappingApprenticeshipsRequest>(r => 
                r.OverlappingApprenticeshipRequests.Count == 2
                && r.OverlappingApprenticeshipRequests[0].Uln == _exampleApprenticships[0].ULN
                && r.OverlappingApprenticeshipRequests[0].StartDate == _exampleApprenticships[0].StartDate.Value
                && r.OverlappingApprenticeshipRequests[0].EndDate == _exampleApprenticships[0].EndDate.Value
                && r.OverlappingApprenticeshipRequests[1].Uln == _exampleApprenticships[1].ULN
                && r.OverlappingApprenticeshipRequests[1].StartDate == _exampleApprenticships[1].StartDate.Value
                && r.OverlappingApprenticeshipRequests[1].EndDate == _exampleApprenticships[1].EndDate.Value
            )), Times.Once);
        }

        [Test]
        public void ThenShouldThrowExceptionIfAnyOverlapsExist()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse { Data = new List<OverlappingApprenticeship>
                {
                    new OverlappingApprenticeship
                    {
                        Apprenticeship = _exampleValidRequest.Apprenticeships.First(),
                        ValidationFailReason = ValidationFailReason.OverlappingEndDate
                    }
                } });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }
    }
}
