using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateProviderPaymentsPriority
{
    [TestFixture]
    public sealed class WhenSavingNewPriorityList
    {
        private UpdateProviderPaymentsPriorityCommand _validCommand;
        private UpdateProviderPaymentsPriorityCommandHandler _handler;
        private Mock<IProviderPaymentRepository> _mockProviderPaymentRepository;

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

            var validator = new UpdateProviderPaymentsPriorityCommandValidator();
            _mockProviderPaymentRepository = new Mock<IProviderPaymentRepository>();

            _handler = new UpdateProviderPaymentsPriorityCommandHandler(validator, _mockProviderPaymentRepository.Object);
        }

        [Test]
        public async Task ShouldCallTheProviderPaymentRepository()
        {
            await _handler.Handle(_validCommand);

            _mockProviderPaymentRepository.Verify(x => x.UpdateProviderPaymentPriority(It.IsAny<long>(), It.IsAny<IList<ProviderPaymentPriorityUpdateItem>>()));
        }
    }
}
