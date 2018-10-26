using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprovedApprenticeship
{
    [TestFixture]
    public class WhenGettingApprovedApprenticeship
    {
        private GetApprovedApprenticeshipQueryHandler _handler;
        private Mock<IApprovedApprenticeshipRepository> _repository;
        private ApprovedApprenticeship _apprenticeship;
        private Mock<AbstractValidator<GetApprovedApprenticeshipRequest>> _validator;
        private GetApprovedApprenticeshipRequest _validRequest;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<AbstractValidator<GetApprovedApprenticeshipRequest>>();
            _validator.Setup(x => x.Validate(It.IsAny<GetApprovedApprenticeshipRequest>()))
                .Returns(new ValidationResult());

            _apprenticeship = new ApprovedApprenticeship
            {
                Id = 1,
                EmployerAccountId = 2,
                ProviderId = 3
            };

            _repository = new Mock<IApprovedApprenticeshipRepository>();
            _repository.Setup(x => x.Get(It.IsAny<long>())).ReturnsAsync(_apprenticeship);

            _validRequest = new GetApprovedApprenticeshipRequest
            {
                ApprenticeshipId = 1,
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 2
                }
            };

            _handler =new GetApprovedApprenticeshipQueryHandler(_repository.Object, _validator.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            await _handler.Handle(TestHelper.Clone(_validRequest));
            _validator.Verify(x => x.Validate(It.IsAny<GetApprovedApprenticeshipRequest>()), Times.Once);
        }

        [Test]
        public void ThenIfValidationFailsThenAnExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<GetApprovedApprenticeshipRequest>())).Returns(
                new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("TEST", "ERROR")
                }));

            Func<Task> act = async () => await _handler.Handle(TestHelper.Clone(_validRequest));

            act.ShouldThrow<ValidationException>();
        }

        [TestCase(CallerType.Employer, 9, true)]
        [TestCase(CallerType.Provider, 9, true)]
        [TestCase(CallerType.Employer, 2, false)]
        [TestCase(CallerType.Provider, 3, false)]
        public void ThenTheCallerIsAuthorised(CallerType callerType, long callerId, bool expectError)
        {
            _validRequest.Caller.CallerType = callerType;
            _validRequest.Caller.Id = callerId;

            Func<Task> act = async () => await _handler.Handle(TestHelper.Clone(_validRequest));

            if (expectError)
            {
                act.ShouldThrow<UnauthorizedException>();
            }
            else
            {
                act.ShouldNotThrow<UnauthorizedException>();
            }
        }
 
        [Test]
        public async Task ThenTheDomainObjectIsReturnedInTheResponse()
        {
            var result = await _handler.Handle(TestHelper.Clone(_validRequest));

            Assert.AreEqual(_apprenticeship, result.Data);
        }
    }
}
