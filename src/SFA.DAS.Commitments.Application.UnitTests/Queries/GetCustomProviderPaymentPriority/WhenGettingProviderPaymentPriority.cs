using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCustomProviderPaymentPriority
{
    [TestFixture]
    public class WhenGettingProviderPaymentPriority
    {
        private GetProviderPaymentsPriorityQueryHandler _handler;
        private Mock<IProviderPaymentRepository> _mockProviderPaymentRepository;

        [SetUp]
        public void Setup()
        {
            var validator = new GetProviderPaymentsPriorityValidator();
            _mockProviderPaymentRepository = new Mock<IProviderPaymentRepository>();

            _handler = new GetProviderPaymentsPriorityQueryHandler(_mockProviderPaymentRepository.Object, validator);
        }

        [Test]
        public void ThenEmployerAccountIdThowsExceptionIfNotValid()
        {
            var request = new GetProviderPaymentsPriorityRequest { EmployerAccountId = 0 };

            Func<Task<GetProviderPaymentsPriorityResponse>> act = async () => await _handler.Handle(request);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenShouldCallTheRepostory()
        {
            var request = new GetProviderPaymentsPriorityRequest { EmployerAccountId = 123L };
            _mockProviderPaymentRepository.Setup(x => x.GetCustomProviderPaymentPriority(request.EmployerAccountId))
                .ReturnsAsync(new List<ProviderPaymentPriorityItem>());

            var result = await _handler.Handle(request);

            _mockProviderPaymentRepository.Verify(x => x.GetCustomProviderPaymentPriority(request.EmployerAccountId), Times.Once);
        }
    }
}
