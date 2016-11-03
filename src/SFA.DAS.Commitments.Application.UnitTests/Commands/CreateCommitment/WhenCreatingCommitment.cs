using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenCreatingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateCommitmentCommandHandler _handler;
        private CreateCommitmentCommand _exampleValidRequest;
        private Mock<IHashingService> _mockHashingService;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockHashingService = new Mock<IHashingService>();
            _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object, _mockHashingService.Object, new CreateCommitmentValidator());

            Fixture fixture = new Fixture();
            fixture.Customize<Api.Types.Apprenticeship>(ob => ob
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
            );
            var populatedCommitment = fixture.Build<Api.Types.Commitment>().Create();
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

            act.ShouldThrow<ValidationException>();
        }

        private void AssertMappingIsCorrect(Domain.Commitment argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Name.Should().Be(_exampleValidRequest.Commitment.Name);
            argument.EmployerAccountId.Should().Be(_exampleValidRequest.Commitment.EmployerAccountId);
            argument.LegalEntityCode.Should().Be(_exampleValidRequest.Commitment.LegalEntityCode);
            argument.ProviderId.Should().Be(_exampleValidRequest.Commitment.ProviderId);
            argument.Status.Should().Be(CommitmentStatus.Draft);
            argument.Apprenticeships.Should().HaveSameCount(_exampleValidRequest.Commitment.Apprenticeships);
            argument.Apprenticeships[0].Id.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Id);
            argument.Apprenticeships[0].ULN.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].ULN);
            argument.Apprenticeships[0].FirstName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].FirstName);
            argument.Apprenticeships[0].LastName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].LastName);
            argument.Apprenticeships[0].CommitmentId.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Apprenticeships[0].Cost.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Cost);
            argument.Apprenticeships[0].AgreementStatus.Should().Be((AgreementStatus)_exampleValidRequest.Commitment.Apprenticeships[0].AgreementStatus);
            argument.Apprenticeships[0].Status.Should().Be((ApprenticeshipStatus)_exampleValidRequest.Commitment.Apprenticeships[0].Status);
            argument.Apprenticeships[0].TrainingType.Should().Be((TrainingType)_exampleValidRequest.Commitment.Apprenticeships[0].TrainingType);
            argument.Apprenticeships[0].TrainingCode.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].TrainingCode);
            argument.Apprenticeships[0].TrainingName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].TrainingName);
            argument.Apprenticeships[0].StartDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].StartDate);
            argument.Apprenticeships[0].EndDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].EndDate);
        }
    }
}
