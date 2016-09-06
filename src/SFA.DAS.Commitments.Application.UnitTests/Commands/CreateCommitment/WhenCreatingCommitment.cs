using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenCreatingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateCommitmentCommandHandler _handler;
        private CreateCommitmentCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object, new CreateCommitmentValidator());

            Fixture fixture = new Fixture();
            var populatedCommitment = fixture.Build<Commitment>().Create();
            _exampleValidRequest = new CreateCommitmentCommand { Commitment = populatedCommitment };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.Create(It.IsAny<Domain.Commitment>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithCommitmentMappedFromRequest()
        {
            Domain.Commitment argument = null;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Domain.Commitment>()))
                .ReturnsAsync(4)
                .Callback<Domain.Commitment>(x => argument = x);

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheCommitmentIdReturnedFromRepository()
        {
            const long ExpectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Domain.Commitment>())).ReturnsAsync(ExpectedCommitmentId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(ExpectedCommitmentId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Commitment.Name = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidRequestException>();
        }

        private void AssertMappingIsCorrect(Domain.Commitment argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Name.Should().Be(_exampleValidRequest.Commitment.Name);
            argument.EmployerAccountId.Should().Be(_exampleValidRequest.Commitment.EmployerAccountId);
            argument.LegalEntityId.Should().Be(_exampleValidRequest.Commitment.LegalEntityId);
            argument.ProviderId.Should().Be(_exampleValidRequest.Commitment.ProviderId);
            argument.Apprenticeships.Should().HaveSameCount(_exampleValidRequest.Commitment.Apprenticeships);
            argument.Apprenticeships[0].Id.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Id);
            argument.Apprenticeships[0].ULN.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].ULN);
            argument.Apprenticeships[0].ApprenticeName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].ApprenticeName);
            argument.Apprenticeships[0].CommitmentId.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Apprenticeships[0].Cost.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Cost);
            argument.Apprenticeships[0].AgreementStatus.Should().Be((Domain.AgreementStatus)_exampleValidRequest.Commitment.Apprenticeships[0].AgreementStatus);
            argument.Apprenticeships[0].Status.Should().Be((Domain.ApprenticeshipStatus)_exampleValidRequest.Commitment.Apprenticeships[0].Status);
            argument.Apprenticeships[0].TrainingId.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].TrainingId);
            argument.Apprenticeships[0].StartDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].StartDate);
            argument.Apprenticeships[0].EndDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].EndDate);
        }
    }
}
