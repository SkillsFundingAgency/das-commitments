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
using SFA.DAS.Commitments.Domain.Entities;
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
            _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object, _mockHashingService.Object, new CreateCommitmentValidator(), Mock.Of<ICommitmentsLogger>());

            Fixture fixture = new Fixture();
            fixture.Customize<Api.Types.Apprenticeship>(ob => ob
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
                .With(x => x.NINumber, ApprenticeshipTestDataHelper.CreateValidNino())
                .With(x => x.FirstName, "First name")
                .With(x => x.FirstName, "Last name")
                .With(x => x.ProviderRef, "Provider ref")
                .With(x => x.EmployerRef, null)
                .With(x => x.StartDate, DateTime.Now.AddYears(5))
                .With(x => x.EndDate, DateTime.Now.AddYears(7))
            );
            var populatedCommitment = fixture.Build<Api.Types.Commitment>().Create();
            _exampleValidRequest = new CreateCommitmentCommand { Commitment = populatedCommitment };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.Create(It.IsAny<Commitment>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithCommitmentMappedFromRequest()
        {
            Commitment argument = null;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>()))
                .ReturnsAsync(4)
                .Callback<Commitment>(x => argument = x);

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheCommitmentIdReturnedFromRepository()
        {
            const long ExpectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>())).ReturnsAsync(ExpectedCommitmentId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(ExpectedCommitmentId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Commitment.Reference = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        private void AssertMappingIsCorrect(Commitment argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Reference.Should().Be(_exampleValidRequest.Commitment.Reference);
            argument.EmployerAccountId.Should().Be(_exampleValidRequest.Commitment.EmployerAccountId);
            argument.LegalEntityId.Should().Be(_exampleValidRequest.Commitment.LegalEntityId);
            argument.ProviderId.Should().Be(_exampleValidRequest.Commitment.ProviderId);
            argument.CommitmentStatus.Should().Be(CommitmentStatus.New);
            argument.LastAction.Should().Be(LastAction.None);
            argument.EmployerLastUpdateInfo.Name.Should().Be(_exampleValidRequest.Commitment.EmployerLastUpdateInfo.Name);
            argument.EmployerLastUpdateInfo.EmailAddress.Should().Be(_exampleValidRequest.Commitment.EmployerLastUpdateInfo.EmailAddress);
            argument.Apprenticeships.Should().HaveSameCount(_exampleValidRequest.Commitment.Apprenticeships);
            argument.Apprenticeships[0].Id.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Id);
            argument.Apprenticeships[0].ULN.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].ULN);
            argument.Apprenticeships[0].FirstName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].FirstName);
            argument.Apprenticeships[0].LastName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].LastName);
            argument.Apprenticeships[0].CommitmentId.Should().Be(_exampleValidRequest.Commitment.Id);
            argument.Apprenticeships[0].Cost.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].Cost);
            argument.Apprenticeships[0].AgreementStatus.Should().Be((AgreementStatus)_exampleValidRequest.Commitment.Apprenticeships[0].AgreementStatus);
            argument.Apprenticeships[0].PaymentStatus.Should().Be((PaymentStatus)_exampleValidRequest.Commitment.Apprenticeships[0].PaymentStatus);
            argument.Apprenticeships[0].TrainingType.Should().Be((TrainingType)_exampleValidRequest.Commitment.Apprenticeships[0].TrainingType);
            argument.Apprenticeships[0].TrainingCode.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].TrainingCode);
            argument.Apprenticeships[0].TrainingName.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].TrainingName);
            argument.Apprenticeships[0].StartDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].StartDate);
            argument.Apprenticeships[0].EndDate.Should().Be(_exampleValidRequest.Commitment.Apprenticeships[0].EndDate);
        }
    }
}
