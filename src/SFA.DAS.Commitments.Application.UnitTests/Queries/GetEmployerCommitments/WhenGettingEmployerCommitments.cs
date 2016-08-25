using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetEmployerCommitments
{
    [TestFixture]
    public class WhenGettingEmployerCommitments
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetEmployerCommitmentsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetEmployerCommitmentsQueryHandler(_mockCommitmentRespository.Object, new GetEmployerCommitmentsValidator());
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 123 });

            _mockCommitmentRespository.Verify(x => x.GetByEmployer(It.IsAny<long>()), Times.Once);
        }

        // TODO: LWA - Review test
        //[Test, AutoData]
        //public async Task ThenShouldReturnListOfCommitmentsInResponse(IList<Commitment> commitmentsFromRepository)
        //{
        //    _mockCommitmentRespository.Setup(x => x.GetByEmployer(It.IsAny<long>())).Returns(Task.FromResult(commitmentsFromRepository));

        //    var response = await _handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 123 });

            
        //    response.Data.Should().BeSameAs(commitmentsFromRepository);
        //}

        [Test]
        public async Task ThenShouldSetHasErrorIndicatorOnResponseIfValidationFails()
        {
            var response = await _handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 0 }); // 0 will fail validation

            response.HasError.Should().BeTrue();
        }
    }
}
