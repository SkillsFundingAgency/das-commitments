using System;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.CommitmentsV2.Types;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.ProviderApproveCohort
{
    [TestFixture]
    public class WhenAProviderApprovesACohort : ApproveCohortTestBase<ProviderApproveCohortCommand>
    {
        [SetUp]
        public void SetUp()
        {
            Validator = new ProviderApproveCohortCommandValidator();
            Command = new ProviderApproveCohortCommand { Caller = new Caller(213, CallerType.Provider), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();
            Commitment = CreateCommitment(Command.CommitmentId, 11234, Command.Caller.Id);
            Commitment.EditStatus = EditStatus.ProviderOnly;
            Account = CreateAccount(Commitment.EmployerAccountId, ApprenticeshipEmployerType.Levy);
            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            EmployerAccountsService.Setup(x => x.GetAccount(Commitment.EmployerAccountId)).ReturnsAsync(Account);
            SetupSuccessfulOverlapCheck();
            V2EventsPublisher.Setup(x => x.SendProviderApproveCohortCommand(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserInfo>()))
                .Returns(Task.CompletedTask);

            Target = new ProviderApproveCohortCommandHandler(Validator,
                CommitmentRepository.Object,
                V2EventsPublisher.Object);
        }

        [Test]
        public void ThenIfValidationFailsTheCommitmentCannotBeApproved()
        {
            Command.Caller = null;

            Assert.ThrowsAsync<ValidationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentCanOnlyBeEditedByEmployerItCannotBeApproved()
        {
            Commitment.EditStatus = EditStatus.EmployerOnly;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCallerIsNotTheProviderForTheCommitmentItCannotBeApproved()
        {
            Commitment.ProviderId = Command.Caller.Id + 1;

            Assert.ThrowsAsync<UnauthorizedException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentIsNotReadyToBeApprovedByTheProviderItCannotBeApproved()
        {
            Commitment.ProviderCanApproveCommitment = false;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public async Task ThenAProviderApproveCohortCommandIsSent()
        {
            await Target.Handle(Command);
            V2EventsPublisher.Verify(x => x.SendProviderApproveCohortCommand(Command.CommitmentId,
                It.Is<string>(m => m == Command.Message),
                It.Is<UserInfo>(u =>
                    u.UserId == Command.UserId &&
                    u.UserDisplayName == Command.LastUpdatedByName &&
                    u.UserEmail == Command.LastUpdatedByEmail)));
        }

        [Test]
        public async Task If_CohortIsAChangePartyRequest_Then_SendUpdateOfPartyRequestCommandIsSent()
        {
            Commitment.ChangeOfPartyRequestId = 100;

            await Target.Handle(Command);

            V2EventsPublisher.Verify(x => x.PublishCohortWithChangeOfPartyUpdatedEvent(Command.CommitmentId,
                It.Is<UserInfo>(u =>
                    u.UserId == Command.UserId &&
                    u.UserDisplayName == Command.LastUpdatedByName &&
                    u.UserEmail == Command.LastUpdatedByEmail)));
        }

        [Test]
        public async Task If_CohortIsNotAChangePartyRequest_Then_SendUpdateOfPartyRequestCommandIsNotSent()
        {
            await Target.Handle(Command);

            V2EventsPublisher.Verify(x => x.PublishCohortWithChangeOfPartyUpdatedEvent(It.IsAny<long>(), It.IsAny<UserInfo>()), Times.Never());
        }
    }
}
