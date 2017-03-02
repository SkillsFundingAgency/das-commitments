using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.DeleteCommitment
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private AbstractValidator<DeleteCommitmentCommand> _validator;
        private DeleteCommitmentCommandHandler _handler;
        private DeleteCommitmentCommand _validCommand;

        [SetUp]
        public void Setup()
        {
            _validator = new DeleteCommitmentValidator();
            _handler = new DeleteCommitmentCommandHandler(Mock.Of<ICommitmentRepository>(), _validator, Mock.Of<ICommitmentsLogger>(), Mock.Of<IHistoryRepository>());

            _validCommand = new DeleteCommitmentCommand { CommitmentId = 2, Caller = new Domain.Caller { Id = 123, CallerType = Domain.CallerType.Provider } };
        }

        [Test]
        public void CommitmentIdShouldBeValid()
        {
            _validCommand.CommitmentId = 0;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<ValidationException>().And.Message.Should().Contain("Commitment Id");
        }

        [Test]
        public void CallerShouldNotBeNull()
        {
            _validCommand.Caller = null;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<ValidationException>().And.Message.Should().Contain("Caller");
        }

        [Test]
        public void CallerIdShouldBeValidForProvider()
        {
            _validCommand.Caller.Id = 0;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<ValidationException>().And.Message.Should().Contain("ProviderId");
        }

        [Test]
        public void CallerIdShouldBeValidForEmployer()
        {
            _validCommand.Caller.CallerType = Domain.CallerType.Employer;
            _validCommand.Caller.Id = 0;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<ValidationException>().And.Message.Should().Contain("AccountId");
        }
    }
}
