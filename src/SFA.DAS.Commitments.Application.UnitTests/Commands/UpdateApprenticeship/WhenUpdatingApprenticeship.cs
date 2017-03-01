using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public sealed class WhenUpdatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private UpdateApprenticeshipCommandHandler _handler;
        private UpdateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<AbstractValidator<UpdateApprenticeshipCommand>> _mockValidator;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();

            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockValidator = new Mock<AbstractValidator<UpdateApprenticeshipCommand>>();
            _handler = new UpdateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object, 
                _mockApprenticeshipRepository.Object, 
                _mockValidator.Object, 
                new ApprenticeshipUpdateRules(), 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>());
            
            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(new ValidationResult());

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Api.Types.Apprenticeship>().Create();

            _exampleValidRequest = new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                ApprenticeshipId = populatedApprenticeship.Id,
                Apprenticeship = populatedApprenticeship,
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                PaymentStatus = PaymentStatus.PendingApproval
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x => x.UpdateApprenticeship(It.IsAny<Apprenticeship>(), It.Is<Caller>(m => m.CallerType == CallerType.Provider), It.IsAny<string>()));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            var validationFailureResult = new ValidationResult(new List<ValidationFailure>() { new ValidationFailure("test", "error text") });
            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(validationFailureResult);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }
    }
}
