﻿using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using Moq;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;
using System;
using SFA.DAS.Commitments.Application.Exceptions;
using Ploeh.AutoFixture;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    [TestFixture]
    public class WhenGettingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetCommitmentQueryHandler _handler;
        private GetCommitmentRequest _exampleValidRequest;
        private Commitment _fakeRepositoryCommitment;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetCommitmentQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentValidator());

            Fixture dataFixture = new Fixture();
            _fakeRepositoryCommitment = dataFixture.Build<Commitment>().Create();
            _exampleValidRequest = new GetCommitmentRequest { CommitmentId = _fakeRepositoryCommitment.Id, ProviderId = _fakeRepositoryCommitment.ProviderId, AccountId = null };
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.GetById(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnACommitmentInResponse()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryCommitment);

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Id.Should().Be(_fakeRepositoryCommitment.Id);
            response.Data.Name.Should().Be(_fakeRepositoryCommitment.Name);
            response.Data.Apprenticeships.Should().HaveSameCount(_fakeRepositoryCommitment.Apprenticeships);
        }

        [Test]
        public void ThenIfCommitmentIdIsZeroItThrowsAnInvalidRequestException()
        {
            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest { CommitmentId = 0 });
            act.ShouldThrow<InvalidRequestException>();
        }

        [Test]
        public async Task ThenReturnsAResponseWithNullIfTheCommitmentIsNotFound()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(default(Commitment));

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Should().BeNull();
        }

        [Test]
        public void ThenAProviderIdThatDoesntMatchTheCommitmentThrowsAnException()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryCommitment);

            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest { CommitmentId = _fakeRepositoryCommitment.Id, ProviderId = _fakeRepositoryCommitment.ProviderId++ });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Provider unauthorized to view commitment: {_fakeRepositoryCommitment.Id}");
        }

        [Test]
        public void ThenAnAccountIdThatDoesntMatchTheCommitmentThrowsAnException()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryCommitment);

            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest { CommitmentId = 5, AccountId = _fakeRepositoryCommitment.EmployerAccountId++ });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Employer unauthorized to view commitment: {_fakeRepositoryCommitment.Id}"); ;
        }
    }
}
