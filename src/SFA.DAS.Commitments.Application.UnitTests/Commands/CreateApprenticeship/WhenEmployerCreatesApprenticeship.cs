using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public sealed class WhenEmployerCreatesApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private CreateApprenticeshipCommandHandler _handler;
        private CreateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IUlnValidator> _mockUlnValidator;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        private Mock<IMessagePublisher> _mockMessagePublisher;

        private long expectedApprenticeshipId = 12;

        private readonly long _providerId = 10012;
        private readonly long _employerAccountId = 10013;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockUlnValidator = new Mock<IUlnValidator>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();

            _mockMessagePublisher.Setup(x => x.PublishAsync(It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()))
                .Returns(() => Task.FromResult(new Unit()));

            var validator = new CreateApprenticeshipValidator(new ApprenticeshipValidator(new StubCurrentDateTime(), _mockUlnValidator.Object, _mockAcademicYearValidator.Object));
            _handler = new CreateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRepository.Object,
                validator,
                _mockApprenticeshipEvents.Object,
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object,
                _mockMessagePublisher.Object);

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>()
                .With(x => x.ULN, "1234567890")
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
                .With(x => x.NINumber, ApprenticeshipTestDataHelper.CreateValidNino())
                .With(x => x.FirstName, "First name")
                .With(x => x.FirstName, "Last name")
                .With(x => x.ProviderRef, "Provider ref")
                .With(x => x.EmployerRef, null)
                .With(x => x.StartDate, DateTime.Now.AddYears(5))
                .With(x => x.EndDate, DateTime.Now.AddYears(7))
                .With(x => x.DateOfBirth, DateTime.Now.AddYears(-16))
                .With(x => x.TrainingCode, string.Empty)
                .With(x => x.TrainingName, string.Empty)
                .Create();

            _mockApprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Domain.Entities.Apprenticeship { Id = expectedApprenticeshipId, ProviderId = _providerId, EmployerAccountId = _employerAccountId });

            _mockCommitmentRespository.Setup(x => x.UpdateCommitment(It.IsAny<Commitment>()))
                .Returns(()=> Task.CompletedTask);

            _exampleValidRequest = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 10013
                },
                CommitmentId = 123L,
                Apprenticeship = populatedApprenticeship,
                UserId = "ABBA123"
            };

        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task ThenIfCohortIsPendingFinalApprovalByEmployerThenAMessageIsEmitted(bool pendingFinalApprovalByEmployer, bool expectEmitEvent)
        {
            //Arrange
            var testCommitment = new Commitment
            {
                EmployerAccountId = _exampleValidRequest.Caller.Id,
                ProviderId = 10012,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = new List<Apprenticeship>
                    {
                        new Apprenticeship
                        {
                            AgreementStatus = pendingFinalApprovalByEmployer
                                ? AgreementStatus.ProviderAgreed
                                : AgreementStatus.NotAgreed
                        }
                    }
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            if (expectEmitEvent)
            {
                _mockMessagePublisher.Verify(
                    x => x.PublishAsync(It.Is<ProviderCohortApprovalUndoneByEmployerUpdate>(
                        m => m.ProviderId == _providerId
                             && m.AccountId == _employerAccountId
                             && m.CommitmentId == _exampleValidRequest.CommitmentId
                        )), Times.Once);
            }
            else
            {
                _mockMessagePublisher.Verify(
                    x => x.PublishAsync(It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()),
                    Times.Never);
            }
        }

        [Test]
        public async Task ThenIfTheCohortIsCurrentlyEmptyThenAMessageToUndoProviderApprovalIsNotEmitted()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                EmployerAccountId = _exampleValidRequest.Caller.Id,
                ProviderId = 10012,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = new List<Apprenticeship>()
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMessagePublisher.Verify(
                x => x.PublishAsync(It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()),
                Times.Never);
        }

        [Test]
        public async Task ThenCohortTransferStatusIsResetIfRejected()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                EmployerAccountId = _exampleValidRequest.Caller.Id,
                ProviderId = 10012,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = new List<Apprenticeship>(),
                TransferApprovalStatus = TransferApprovalStatus.TransferRejected
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
                _mockCommitmentRespository.Verify(x => x.UpdateCommitment(It.Is<Commitment>(c =>
                    c.TransferApprovalStatus == null
                    && c.TransferApprovalActionedOn == null
                )), Times.Once);
        }


        [TestCase(TransferApprovalStatus.Pending)]
        [TestCase(null)]
        public async Task ThenCohortTransferStatusIsNotResetIfNotRejected(TransferApprovalStatus status)
        {
            //Arrange
            var testCommitment = new Commitment
            {
                EmployerAccountId = _exampleValidRequest.Caller.Id,
                ProviderId = 10012,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = new List<Apprenticeship>(),
                TransferApprovalStatus = status
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockCommitmentRespository.Verify(x => x.UpdateCommitment(It.IsAny<Commitment>()), Times.Never);
        }
    }
}
