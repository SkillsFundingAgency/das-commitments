﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateApprenticeshipCommandHandler _handler;
        private CreateApprenticeshipCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new CreateApprenticeshipCommandHandler(_mockCommitmentRespository.Object, new CreateApprenticeshipValidator());

            Fixture fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>()
                .With(x => x.ULN, "1234567890")
                .Create();

            _exampleValidRequest = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeship = populatedApprenticeship
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.CreateApprenticeship(It.IsAny<Domain.Apprenticeship>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithApprenticeshipMappedFromRequest()
        {
            Domain.Apprenticeship argument = null;
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });
            _mockCommitmentRespository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Apprenticeship>()))
                .ReturnsAsync(9)
                .Callback<Domain.Apprenticeship>(x => argument = x);

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheApprenticeshipIdReturnedFromRepository()
        {
            const long ExpectedApprenticeshipId = 88;
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });
            _mockCommitmentRespository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Apprenticeship>())).ReturnsAsync(ExpectedApprenticeshipId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(ExpectedApprenticeshipId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Apprenticeship = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        private void AssertMappingIsCorrect(Domain.Apprenticeship argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.Apprenticeship.Id);
            argument.FirstName.Should().Be(_exampleValidRequest.Apprenticeship.FirstName);
            argument.LastName.Should().Be(_exampleValidRequest.Apprenticeship.LastName);
            argument.CommitmentId.Should().Be(_exampleValidRequest.CommitmentId);
            argument.Cost.Should().Be(_exampleValidRequest.Apprenticeship.Cost);
            argument.StartDate.Should().Be(_exampleValidRequest.Apprenticeship.StartDate);
            argument.EndDate.Should().Be(_exampleValidRequest.Apprenticeship.EndDate);
            argument.TrainingType.Should().Be((TrainingType)_exampleValidRequest.Apprenticeship.TrainingType);
            argument.TrainingCode.Should().Be(_exampleValidRequest.Apprenticeship.TrainingCode);
            argument.TrainingName.Should().Be(_exampleValidRequest.Apprenticeship.TrainingName);
            argument.ULN.Should().Be(_exampleValidRequest.Apprenticeship.ULN);
            argument.Status.Should().Be(Domain.ApprenticeshipStatus.ReadyForApproval);
            argument.AgreementStatus.Should().Be((Domain.AgreementStatus)_exampleValidRequest.Apprenticeship.AgreementStatus);
        }
    }
}
