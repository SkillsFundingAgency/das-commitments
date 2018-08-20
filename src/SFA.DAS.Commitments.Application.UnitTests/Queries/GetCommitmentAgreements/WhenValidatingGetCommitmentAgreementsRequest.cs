using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitmentAgreements
{
    [TestFixture]
    public class WhenValidatingGetCommitmentAgreementsRequest
    {
        private GetCommitmentAgreementsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetCommitmentAgreementsValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void AndTheIdIsZeroOrLessThenIsNotValid(long providerId)
        {
            var result = _validator.Validate(new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.First().PropertyName.Should().Be("Id");
            result.Errors.First().ErrorMessage.Should().Be("Id must be greater than zero.");
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void AndTheIdGreaterThanZeroThenIsValid(long providerId)
        {
            var result = _validator.Validate(new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            result.IsValid.Should().BeTrue();
            result.Errors.Count.Should().Be(0);
        }


        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.TransferReceiver)]
        [TestCase(CallerType.TransferSender)]
        public void AndTheCallerIsntProvidedThenNotValid(CallerType callerType)
        {
            var result = _validator.Validate(new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = callerType,
                    Id = 1
                }
            });

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.First().PropertyName.Should().Be("CallerType");
            result.Errors.First().ErrorMessage.Should().Be("CallerType must be Provider.");
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.TransferReceiver)]
        [TestCase(CallerType.TransferSender)]
        public void AndTheCallerIsntProvidedAndIdIsZeroThenThenNotValid(CallerType callerType)
        {
            var result = _validator.Validate(new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = callerType,
                    Id = 0
                }
            });

            // stops validating after first failure & CallerType takes priority
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.First().PropertyName.Should().Be("CallerType");
            result.Errors.First().ErrorMessage.Should().Be("CallerType must be Provider.");
        }
    }
}
