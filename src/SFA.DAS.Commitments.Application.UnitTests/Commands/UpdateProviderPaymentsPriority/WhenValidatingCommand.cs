using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
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
                ProviderPriorities = new List<ProviderPaymentPriorityUpdateItem>
                {
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 1, ProviderId = 99 },
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 2, ProviderId = 22 },
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 3, ProviderId = 66 },
                }
            };

            _handler = new UpdateProviderPaymentsPriorityCommandHandler(
                new UpdateProviderPaymentsPriorityCommandValidator(), 
                Mock.Of<IProviderPaymentRepository>(), 
                Mock.Of<IMediator>());
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
            _validCommand.ProviderPriorities[1].ProviderId = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ProviderIdsMustAllHaveDistinctValues()
        {
            _validCommand.ProviderPriorities[1].ProviderId = 123;
            _validCommand.ProviderPriorities[2].ProviderId = 123;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void PriorityValuesMustBeUnique()
        {
            _validCommand.ProviderPriorities[1].PriorityOrder = 2;
            _validCommand.ProviderPriorities[2].PriorityOrder = 2;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void PriorityValuesMustBeSequentialStartingFromOne()
        {
            _validCommand.ProviderPriorities[0].PriorityOrder = 1;
            _validCommand.ProviderPriorities[1].PriorityOrder = 2;
            _validCommand.ProviderPriorities[2].PriorityOrder = 4;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrow<ValidationException>();
        }
    }
}
