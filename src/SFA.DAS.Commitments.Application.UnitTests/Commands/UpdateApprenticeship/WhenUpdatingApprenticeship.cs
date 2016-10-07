using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public sealed class WhenUpdatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateApprenticeshipCommandHandler _handler;
        private UpdateApprenticeshipCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new UpdateApprenticeshipCommandHandler(_mockCommitmentRespository.Object, new UpdateApprenticeshipValidator());

            Fixture fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>().Create();
            _exampleValidRequest = new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                ApprenticeshipId = populatedApprenticeship.Id,
                Apprenticeship = populatedApprenticeship
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateApprenticeship(It.IsAny<Domain.Apprenticeship>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithApprenticeshipMappedFromRequest()
        {
            Domain.Apprenticeship argument = null;
            _mockCommitmentRespository.Setup(x => x.GetById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            _mockCommitmentRespository.Setup(x => x.UpdateApprenticeship(It.IsAny<Domain.Apprenticeship>()))
                .Returns(Task.FromResult(default(object))) // Return a fake Task
                .Callback<Domain.Apprenticeship>(x => argument = x);

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Apprenticeship = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        private void AssertMappingIsCorrect(Domain.Apprenticeship argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.ApprenticeshipId);
            argument.FirstName.Should().Be(_exampleValidRequest.Apprenticeship.FirstName);
            argument.LastName.Should().Be(_exampleValidRequest.Apprenticeship.LastName);
            argument.CommitmentId.Should().Be(_exampleValidRequest.CommitmentId);
            argument.Cost.Should().Be(_exampleValidRequest.Apprenticeship.Cost);
            argument.StartDate.Should().Be(_exampleValidRequest.Apprenticeship.StartDate);
            argument.EndDate.Should().Be(_exampleValidRequest.Apprenticeship.EndDate);
            argument.TrainingId.Should().Be(_exampleValidRequest.Apprenticeship.TrainingId);
            argument.ULN.Should().Be(_exampleValidRequest.Apprenticeship.ULN);
            argument.Status.Should().Be((Domain.ApprenticeshipStatus)_exampleValidRequest.Apprenticeship.Status);
            argument.AgreementStatus.Should().Be((Domain.AgreementStatus)_exampleValidRequest.Apprenticeship.AgreementStatus);
        }
    }
}
