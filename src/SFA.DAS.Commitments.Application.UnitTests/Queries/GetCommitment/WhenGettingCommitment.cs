using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using Moq;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    [TestFixture]
    public class WhenGettingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetCommitmentQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetCommitmentQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentValidator());
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetCommitmentRequest { CommitmentId = 3 });

            _mockCommitmentRespository.Verify(x => x.GetById(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenShouldReturnACommitmentInResponse(Commitment commitmentFromRepository)
        {
            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).Returns(Task.FromResult(commitmentFromRepository));

            var response = await _handler.Handle(new GetCommitmentRequest { CommitmentId = 5 });

            response.Data.Should().BeSameAs(commitmentFromRepository);
        }

        [Test]
        public async Task ThenShouldSetHasErrorIndicatorOnResponseIfValidationFails()
        {
            var response = await _handler.Handle(new GetCommitmentRequest { CommitmentId = 0 }); // 0 will fail validation

            response.HasErrors.Should().BeTrue();
        }
    }
}
