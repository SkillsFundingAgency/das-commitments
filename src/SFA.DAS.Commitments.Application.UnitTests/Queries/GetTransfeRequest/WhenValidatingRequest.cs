using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequest;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetTransfeRequest
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetTransferRequestValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetTransferRequestValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheTransferRequestIdIsZeroOrLessIsNotValid(long testId)
        {
            var result = _validator.Validate(new GetTransferRequestRequest
            {
                TransferRequestId = testId,
                Caller = new Caller
                {
                    CallerType = CallerType.TransferSender,
                    Id = 1
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testId)
        {
            var result = _validator.Validate(new GetTransferRequestRequest
            {
                TransferRequestId = 100,
                Caller = new Caller
                {
                    CallerType = CallerType.TransferSender,
                    Id = testId
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenIfTheTransferRequestIdAndAccountIdAresGreaterThanZero()
        {
            var result = _validator.Validate(new GetTransferRequestRequest
            {
                TransferRequestId = 100,
                Caller = new Caller
                {
                    CallerType = CallerType.TransferReceiver,
                    Id = 1
                }
            });

            result.IsValid.Should().BeTrue();
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenEnsureICallerTypeIsValid(CallerType caller)
        {
            var result = _validator.Validate(new GetTransferRequestRequest
            {
                TransferRequestId = 100,
                Caller = new Caller
                {
                    CallerType = caller,
                    Id = 2
                }
            });

            result.IsValid.Should().BeFalse();
        }
    }
}
