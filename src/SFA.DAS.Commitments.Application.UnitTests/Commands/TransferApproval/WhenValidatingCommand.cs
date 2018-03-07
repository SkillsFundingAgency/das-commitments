using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.TransferApproval;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.TransferApproval
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private TransferApprovalValidator _target;
        private TransferApprovalCommand _command;

        [SetUp]
        public void Setup()
        {
            _target = new TransferApprovalValidator();

            _command = new TransferApprovalCommand
            {
                CommitmentId = 123,
                TransferSenderId = 999,
                TransferReceiverId = 777,
                TransferStatus = TransferApprovalStatus.TransferRejected,
                UserEmail = "test@test.com",
                UserName = "Test"
            };
        }

        [TestCase(TransferApprovalStatus.TransferApproved)]
        [TestCase(TransferApprovalStatus.TransferRejected)]
        public void ThenIsValidIfAllFieldsAreSetCorrectly(TransferApprovalStatus status)
        {
            _command.TransferStatus = status;
            var validationResult = _target.Validate(_command);
            Assert.IsTrue(validationResult.IsValid);
        }


        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIsInvalidIfCommitmentIdIsZeroOrLess(long commitmentId)
        {
            _command.CommitmentId = commitmentId;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIsInvalidIfTransferReceieverIdIsZeroOrLess(long transferReceiverId)
        {
            _command.TransferReceiverId = transferReceiverId;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIsInvalidIfTransferSenderIdIsZeroOrLess(long transferSenderId)
        {
            _command.TransferSenderId = transferSenderId;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }
    
        [Test]
        public void ThenIsInvalidIfUserEmailIsNull()
        {
            _command.UserEmail = null;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfUserNameIsNull()
        {
            _command.UserName = null;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfTransferApprovalStatusIsPending()
        {
            _command.TransferStatus = TransferApprovalStatus.Pending;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }
    }
}
