using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Messaging.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using TransferApprovalStatus = SFA.DAS.Commitments.Domain.Entities.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohortWhichHasATransferSender : ApproveCohortTestBase<EmployerApproveCohortCommand>
    {
        private Mock<IMessagePublisher> _messagePublisher;
        private long _transferRequestId = 999;
        
        [SetUp]
        public void SetUp()
        {
            Validator = new EmployerApproveCohortCommandValidator();
            Command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();
            Commitment = CreateCommitment(Command.CommitmentId, Command.Caller.Id, 234587, 1000, "Nice Company");
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);
            Account = CreateAccount(Commitment.EmployerAccountId, ApprenticeshipEmployerType.Levy);
            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            CommitmentRepository.Setup(x => x.StartTransferRequestApproval(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<int>(),
                It.IsAny<List<TrainingCourseSummary>>())).ReturnsAsync(_transferRequestId);
            EmployerAccountsService.Setup(x => x.GetAccount(Commitment.EmployerAccountId)).ReturnsAsync(Account);

            SetupSuccessfulOverlapCheck();
            
            _messagePublisher = new Mock<IMessagePublisher>();

            ApprenticeshipInfoService = new Mock<IApprenticeshipInfoService>();
            ApprenticeshipInfoService.Setup(x => x.GetTrainingProgram(It.IsAny<string>()))
                .ReturnsAsync(new Standard
                {
                    FundingPeriods = new List<FundingPeriod>
                    {
                        new FundingPeriod {FundingCap = 1000}
                    }
                });

            Target = new EmployerApproveCohortCommandHandler(Validator,
                CommitmentRepository.Object,
                ApprenticeshipRepository.Object,
                OverlapRules.Object,
                CurrentDateTime.Object,
                HistoryRepository.Object,
                ApprenticeshipEventsList.Object,
                ApprenticeshipEventsPublisher.Object,
                Mediator.Object,
                _messagePublisher.Object,
                Mock.Of<ICommitmentsLogger>(),
                ApprenticeshipInfoService.Object,
                FeatureToggleService.Object,
                EmployerAccountsService.Object);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedAMessageIsPublishedToTransferSender()
        {
            await Target.Handle(Command);

            _messagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovalByTransferSenderRequested>(y =>
                y.TransferRequestId == _transferRequestId &&
                y.ReceivingEmployerAccountId == Commitment.EmployerAccountId &&
                y.CommitmentId == Commitment.Id && y.SendingEmployerAccountId == Commitment.TransferSenderId)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedNoSetPaymentOrderCommandIsSent()
        {
            await Target.Handle(Command);

            Mediator.Verify(x => x.SendAsync(It.IsAny<SetPaymentOrderCommand>()), Times.Never);
        }
        
        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentApprenticeshipEmployerTypeIsNotSet()
        {
            await Target.Handle(Command);
            
            Assert.IsNull(Commitment.ApprenticeshipEmployerTypeOnApproval);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedNoPriceHistoryIsCreated()
        {
            await Target.Handle(Command);

            ApprenticeshipRepository.Verify(x => x.CreatePriceHistoryForApprenticeshipsInCommitment(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedDoNotSetAStartDateForTheApprenticeshipEventsList()
        {

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenEnsureTheStartATransferRequestInRepositoryIsCalled()
        {
            await Target.Handle(Command);

            CommitmentRepository.Verify(x => x.StartTransferRequestApproval(Commitment.Id,
                It.IsAny<decimal>(), It.IsAny<int>(), It.Is<List<TrainingCourseSummary>>(p =>
                    p.Count == 1 && p[0].ApprenticeshipCount == 2 &&
                    p[0].CourseTitle == Commitment.Apprenticeships[0].TrainingName)));
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedThenEventsEmittedShouldIndicatePendingTransferApproval()
        {
            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(
                It.Is<Commitment>(c => c.TransferApprovalStatus == TransferApprovalStatus.Pending),
                It.IsAny<Apprenticeship>(),
                It.IsAny<string>(),
                null,
                null
            ), Times.Exactly(Commitment.Apprenticeships.Count));
        }


        [Test]
        public async Task ThenTheProviderApprovedCohortNotificationIsNotSent()
        {
            await Target.Handle(Command);
            NotificationsPublisher.Verify(x => x.ProviderApprovedCohort(It.IsAny<Commitment>()), Times.Never);
        }
    }
}
