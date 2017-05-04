using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.DeleteApprenticeship
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private AbstractValidator<DeleteApprenticeshipCommand> _validator;
        private DeleteApprenticeshipCommandHandler _handler;
        private DeleteApprenticeshipCommand _validCommand;

        [SetUp]
        public void Setup()
        {
            _validator = new DeleteApprenticeshipValidator();
            _handler = new DeleteApprenticeshipCommandHandler(Mock.Of<ICommitmentRepository>(), Mock.Of<IApprenticeshipRepository>(), _validator, Mock.Of<ICommitmentsLogger>(), Mock.Of<IApprenticeshipEvents>(), Mock.Of<IHistoryRepository>());

            _validCommand = new DeleteApprenticeshipCommand() { ApprenticeshipId = 2, Caller = new Domain.Caller { Id = 123, CallerType = Domain.CallerType.Provider } };
        }

        [Test]
        public void ApprenticeshipIdShouldBeValid()
        {
            _validCommand.ApprenticeshipId = 0;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<ValidationException>().And.Message.Should().Contain("Apprenticeship Id");
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
