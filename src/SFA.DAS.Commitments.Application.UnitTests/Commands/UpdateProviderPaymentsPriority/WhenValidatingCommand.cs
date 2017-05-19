using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateProviderPaymentsPriority
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private UpdateProviderPaymentsPriorityCommand _validCommand;
        private UpdateProviderPaymentsPriorityCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _validCommand = new UpdateProviderPaymentsPriorityCommand
            {
                EmployerAccountId = 123L,
                ProviderPriorities = new List<long> { 99, 22, 66 }
            };

            _handler = new UpdateProviderPaymentsPriorityCommandHandler(new UpdateProviderPaymentsPriorityCommandValidator());
        }

        [Test]
        public void ShouldThrowExceptionIfAccountIdIsNotValid()
        {
            _validCommand.EmployerAccountId = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ProviderIdsMustAllBeNonZero()
        {
            _validCommand.ProviderPriorities = new List<long> { 22, 33, 0, 55 };

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ProviderIdsMustAllHaveDistinctValues()
        {
            _validCommand.ProviderPriorities = new List<long> { 22, 33, 22, 55 };

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }
    }
}
