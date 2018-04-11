using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public class WhenEmployerUpdatesApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private UpdateApprenticeshipCommandHandler _handler;
        private UpdateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<AbstractValidator<UpdateApprenticeshipCommand>> _mockValidator;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IApprenticeshipUpdateRules> _mockApprenticeshipUpdateRules;
        private Mock<IMessagePublisher> _mockMessagePublisher;
        private Commitment _testCommitment;
        private ProviderCohortApprovalUndoneByEmployerUpdate _emittedEvent;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockValidator = new Mock<AbstractValidator<UpdateApprenticeshipCommand>>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockApprenticeshipUpdateRules = new Mock<IApprenticeshipUpdateRules>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();

            _handler = new UpdateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRepository.Object,
                _mockValidator.Object,
                _mockApprenticeshipUpdateRules.Object,
                _mockApprenticeshipEvents.Object,
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object,
                _mockMessagePublisher.Object
                );

            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(new ValidationResult());

            _mockMessagePublisher.Setup(x => x.PublishAsync(It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()))
                .Callback<object>((obj) => _emittedEvent = obj as ProviderCohortApprovalUndoneByEmployerUpdate)
                .Returns(() => Task.FromResult(new Unit()));

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>().Create();

            _exampleValidRequest = new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 111L
                },
                CommitmentId = 123L,
                ApprenticeshipId = populatedApprenticeship.Id,
                Apprenticeship = populatedApprenticeship,
                UserName = "Bob"
            };
        }

        [TearDown]
        public void TearDown()
        {
            _emittedEvent = null;
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task ThenIfCohortIsPendingFinalApprovalByEmployerThenAMessageIsEmitted(bool pendingFinalApprovalByEmployer, bool expectEmitEvent)
        {
            //Arrange
            var testCommitment = new Commitment
            {
                EmployerAccountId = _exampleValidRequest.Caller.Id,
                ProviderId = 123L,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = new List<Apprenticeship>
                    {
                        new Apprenticeship
                        {
                            Id = _exampleValidRequest.ApprenticeshipId,
                            AgreementStatus = pendingFinalApprovalByEmployer
                                ? AgreementStatus.ProviderAgreed
                                : AgreementStatus.NotAgreed
                        }
                    }
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId))
                .ReturnsAsync(testCommitment.Apprenticeships.First());

            _mockApprenticeshipUpdateRules.Setup(x => x.DetermineNewAgreementStatus(
                    It.IsAny<AgreementStatus>(),
                    It.IsAny<CallerType>(),
                    It.IsAny<bool>()))
                .Returns(() => AgreementStatus.NotAgreed);

            _mockApprenticeshipUpdateRules.Setup(x => x.DetermineWhetherChangeRequiresAgreement(
                    It.IsAny<Apprenticeship>(),
                    It.IsAny<Apprenticeship>()))
            .Returns(true);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            if (expectEmitEvent)
            {
                Assert.IsNotNull(_emittedEvent, "Event expected but not emitted");
                Assert.AreEqual(testCommitment.ProviderId, _emittedEvent.ProviderId);
                Assert.AreEqual(testCommitment.EmployerAccountId, _emittedEvent.AccountId);
                Assert.AreEqual(_exampleValidRequest.CommitmentId, _emittedEvent.CommitmentId);
            }
            else
            {
                Assert.IsNull(_emittedEvent, "Unexpected event emitted");
            }
        }
    }
}
