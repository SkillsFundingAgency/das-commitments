using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Client;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public sealed class WhenUpdatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateApprenticeshipCommandHandler _handler;
        private UpdateApprenticeshipCommand _exampleValidRequest;
        private Mock<IEventsApi> _mockEventsApi;

        [SetUp]
        public void SetUp()
        {
            _mockEventsApi = new Mock<IEventsApi>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new UpdateApprenticeshipCommandHandler(_mockCommitmentRespository.Object, new UpdateApprenticeshipValidator(), _mockEventsApi.Object, new ApprenticeshipUpdateRules());

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
            _mockCommitmentRespository.Setup(x => x.GetById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                PaymentStatus = PaymentStatus.PendingApproval
            });

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>(), It.Is<Caller>(m => m.CallerType == CallerType.Provider)));
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

        private void AssertMappingIsCorrect(Domain.Entities.Apprenticeship argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.ApprenticeshipId);
            argument.FirstName.Should().Be(_exampleValidRequest.Apprenticeship.FirstName);
            argument.LastName.Should().Be(_exampleValidRequest.Apprenticeship.LastName);
            argument.CommitmentId.Should().Be(_exampleValidRequest.CommitmentId);
            argument.Cost.Should().Be(_exampleValidRequest.Apprenticeship.Cost);
            argument.StartDate.Should().Be(_exampleValidRequest.Apprenticeship.StartDate);
            argument.EndDate.Should().Be(_exampleValidRequest.Apprenticeship.EndDate);
            argument.TrainingType.Should().Be((TrainingType) _exampleValidRequest.Apprenticeship.TrainingType);
            argument.TrainingCode.Should().Be(_exampleValidRequest.Apprenticeship.TrainingCode);
            argument.TrainingName.Should().Be(_exampleValidRequest.Apprenticeship.TrainingName);
            argument.ULN.Should().Be(_exampleValidRequest.Apprenticeship.ULN);
            argument.PaymentStatus.Should().Be((PaymentStatus) _exampleValidRequest.Apprenticeship.PaymentStatus);
            argument.AgreementStatus.Should().Be((AgreementStatus) _exampleValidRequest.Apprenticeship.AgreementStatus);
        }
    }
}
