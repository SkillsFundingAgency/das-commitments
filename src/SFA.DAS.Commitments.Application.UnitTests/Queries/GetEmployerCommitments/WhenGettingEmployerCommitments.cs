using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetEmployerCommitments
{
    [TestFixture]
    public class WhenGettingEmployerCommitments
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetCommitmentsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetCommitmentsQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentsValidator());
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 123
                }
            });

            _mockCommitmentRespository.Verify(x => x.GetCommitmentsByEmployer(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenShouldReturnListOfCommitmentsInResponse(IList<CommitmentSummary> commitmentsFromRepository)
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentsByEmployer(It.IsAny<long>())).ReturnsAsync(commitmentsFromRepository);

            var response = await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 123
                }
            });

            response.Data.Should().HaveSameCount(commitmentsFromRepository);
            commitmentsFromRepository.Should().OnlyContain(x => response.Data.Any(y => y.Id == x.Id && y.Reference == x.Reference));
        }

        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        public async Task ThenShouldReturnListOfCommitmentsInResponseWithAgreementStatusAndCount(AgreementStatus agreementStatus)
        {
            var fixture = new Fixture();
            
            fixture.Customize<CommitmentSummary>(ob => ob
                .With(x => x.AgreementStatus, agreementStatus));

            var commitment = fixture.Create<CommitmentSummary>();
            commitment.ApprenticeshipCount = 3;

            IList<CommitmentSummary> commitmentsFromRepository = new List<CommitmentSummary> { commitment };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentsByEmployer(It.IsAny<long>())).ReturnsAsync(commitmentsFromRepository);

            var response = await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 123
                }
            });

            response.Data.Should().HaveSameCount(commitmentsFromRepository);
            response.Data[0].Messages.Should().HaveSameCount(commitmentsFromRepository[0].Messages);
            commitmentsFromRepository.Should()
                .OnlyContain(x => response.Data.All(y =>
                   y.AgreementStatus == agreementStatus
                && y.ApprenticeshipCount == x.ApprenticeshipCount ));
        }

        [Test]
        public void ThenShouldThrowInvalidRequestExceptionIfValidationFails()
        {
            Func<Task> act = async () => await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 0
                }
            });

            act.ShouldThrow<ValidationException>();
        }
    }
}
