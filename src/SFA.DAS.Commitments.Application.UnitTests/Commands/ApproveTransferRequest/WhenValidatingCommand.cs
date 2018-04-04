using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.ApproveTransferRequest
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private ApproveTransferRequestValidator _target;
        private ApproveTransferRequestCommand _command;

        [SetUp]
        public void Setup()
        {
            _target = new ApproveTransferRequestValidator();

            _command = new ApproveTransferRequestCommand
            {
                CommitmentId = 123,
                TransferSenderId = 999,
                TransferReceiverId = 777,
                UserEmail = "test@test.com",
                UserName = "Test"
            };
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
    }
}
