using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingACommitmentView
    {
        private ICommitmentRules _rules;
        private CommitmentMapper _mapper;

        [SetUp]
        public void Arrange()
        {
            _rules = Mock.Of<ICommitmentRules>();
            _mapper = new CommitmentMapper(_rules);
        }

        [TestCase(CallerType.Employer, true, false, TransferApprovalStatus.Pending)]
        [TestCase(CallerType.Provider,false, true, TransferApprovalStatus.Pending)]
        [TestCase(CallerType.TransferSender, false, false, TransferApprovalStatus.Pending)]
        public void ThenTransferSenderFieldsAreMappedCorrectly(CallerType callerType, bool canEmployerApprove, bool canProviderApprove, TransferApprovalStatus transferStatus)
        {
            var commitment = new Commitment
            {
                TransferSenderId = 1,
                TransferSenderName = "Transfer Sender Org",
                TransferApprovalStatus = transferStatus,
                TransferApprovalActionedOn = new DateTime(2018, 09, 09),
                TransferApprovalActionedByEmployerName = "Name",
                ProviderCanApproveCommitment = canProviderApprove,
                EmployerCanApproveCommitment = canEmployerApprove,
            };

            var result = _mapper.MapFrom(commitment, callerType);

            Assert.AreEqual(commitment.TransferSenderId, result.TransferSenderInfo.TransferSenderId);
            Assert.AreEqual(commitment.TransferSenderName, result.TransferSenderInfo.TransferSenderName);
            Assert.AreEqual(commitment.TransferApprovalStatus, (TransferApprovalStatus)result.TransferSenderInfo.TransferApprovalStatus);
            Assert.AreEqual(commitment.TransferApprovalActionedOn, result.TransferSenderInfo.TransferApprovalSetOn);
            Assert.AreEqual(commitment.TransferApprovalActionedByEmployerName, result.TransferSenderInfo.TransferApprovalSetBy);
            Assert.AreEqual(true, result.CanBeApproved);
        }
    }
}
