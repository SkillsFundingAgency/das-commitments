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
                ProviderCanApproveCommitment = canProviderApprove,
                EmployerCanApproveCommitment = canEmployerApprove,
            };

            var result = _mapper.MapFrom(commitment, callerType);

            Assert.AreEqual(1, result.TransferSender.Id);
            Assert.AreEqual("Transfer Sender Org", result.TransferSender.Name);
            Assert.AreEqual(transferStatus, (TransferApprovalStatus)result.TransferSender.TransferApprovalStatus);
            Assert.AreEqual(new DateTime(2018, 09, 09), result.TransferSender.TransferApprovalSetOn);
            Assert.AreEqual(true, result.CanBeApproved);
        }
    }
}
