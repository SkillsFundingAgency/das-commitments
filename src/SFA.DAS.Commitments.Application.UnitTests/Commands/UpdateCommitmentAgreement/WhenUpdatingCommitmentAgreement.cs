using System;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentAgreement
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentAgreement
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateCommitmentAgreementCommandHandler _handler;

        [Test]
        public void Setup()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new UpdateCommitmentAgreementCommandHandler(_mockCommitmentRespository.Object, new ApprenticeshipUpdateRules(), Mock.Of<IApprenticeshipEvents>(), Mock.Of<ICommitmentsLogger>(), Mock.Of<IMediator>());
        }

        [Test]
        public void ShouldThrowAnExceptionIfEmployerTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = false, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var command = new UpdateCommitmentAgreementCommand
            {
                Caller = new Domain.Caller { Id = 444, CallerType = Domain.CallerType.Employer },
                LatestAction = Api.Types.LastAction.Approve,
                CommitmentId = 123L
            };

            Func<Task> act = async () => { await _handler.Handle(command); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }

        [Test]
        public void ShouldThrowAnExceptionIfProviderTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 333, ProviderCanApproveCommitment = false, EditStatus = EditStatus.ProviderOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var command = new UpdateCommitmentAgreementCommand
            {
                Caller = new Domain.Caller { Id = 333, CallerType = Domain.CallerType.Provider },
                LatestAction = Api.Types.LastAction.Approve,
                CommitmentId = 123L
            };

            Func<Task> act = async () => { await _handler.Handle(command); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }
    }
}
