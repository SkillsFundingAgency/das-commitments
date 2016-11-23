using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using Moq;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;
using System;
using FluentValidation;
using SFA.DAS.Commitments.Application.Exceptions;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    using System.Collections.Generic;
    using System.Linq;

    using SFA.DAS.Commitments.Application.Queries.GetCommitments;
    using SFA.DAS.Commitments.Application.Rules;

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
            _handler = new GetCommitmentQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentValidator(), new CommitmentRules());

            Fixture dataFixture = new Fixture();
            _fakeRepositoryCommitment = dataFixture.Build<Commitment>().Create();
            _exampleValidRequest = new GetCommitmentRequest
            {
                CommitmentId = _fakeRepositoryCommitment.Id,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = _fakeRepositoryCommitment.ProviderId.Value
                }
            };
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
            response.Data.Reference.Should().Be(_fakeRepositoryCommitment.Reference);
            response.Data.Apprenticeships.Should().HaveSameCount(_fakeRepositoryCommitment.Apprenticeships);
        }

        [Test]
        public void ThenIfCommitmentIdIsZeroItThrowsAnInvalidRequestException()
        {
            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest
            {
                CommitmentId = 0,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                }
            });
            act.ShouldThrow<ValidationException>();
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

            var providerId = _fakeRepositoryCommitment.ProviderId++.Value;

            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest
            {
                CommitmentId = _fakeRepositoryCommitment.Id,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Provider {providerId} unauthorized to view commitment {_fakeRepositoryCommitment.Id}");
        }

        [Test]
        public void ThenAnAccountIdThatDoesntMatchTheCommitmentThrowsAnException()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryCommitment);

            var employerAccountId = _fakeRepositoryCommitment.EmployerAccountId++;

            Func<Task> act = async () => await _handler.Handle(new GetCommitmentRequest
            {
                CommitmentId = _fakeRepositoryCommitment.Id,
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = employerAccountId
                }
            });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Employer {employerAccountId} unauthorized to view commitment {_fakeRepositoryCommitment.Id}"); ;
        }

        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        public async Task ThenShouldReturnListOfCommitmentsInResponseWithAgreementStatusAndCount(AgreementStatus agreementStatus)
        {
            var fixture = new Fixture();

            fixture.Customize<Apprenticeship>(ob => ob
                .With(x => x.AgreementStatus, agreementStatus));

            var commitment = fixture.Create<Commitment>();
            commitment.EmployerAccountId = 123L;

            commitment.Apprenticeships = new List<Apprenticeship>
            {
                fixture.Create<Apprenticeship>(),
                fixture.Create<Apprenticeship>(),
                fixture.Create<Apprenticeship>()
            };

            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(commitment);

            var response = await _handler.Handle(new GetCommitmentRequest
            {
                CommitmentId = 123L,
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 123
                }
            });

            commitment.Apprenticeships.Should()
                .OnlyContain(x => response.Data.Apprenticeships.All(y =>
                   y.AgreementStatus == (Api.Types.AgreementStatus)agreementStatus ));
        }
    }
}
