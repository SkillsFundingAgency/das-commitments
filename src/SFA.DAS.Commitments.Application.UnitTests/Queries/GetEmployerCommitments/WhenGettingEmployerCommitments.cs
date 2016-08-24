using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetEmployerCommitments
{
    [TestFixture]
    public class WhenGettingEmployerCommitments
    {
        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            var mockCommitmentRespository = new Mock<ICommitmentRepository>();

            var handler = new GetEmployerCommitmentsQueryHandler(mockCommitmentRespository.Object, new GetEmployerCommitmentsValidator());

            await handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 123 });

            mockCommitmentRespository.Verify(x => x.GetByEmployer(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenShouldReturnListOfCommitmentsInResponse(IList<Commitment> commitmentsFromRepository)
        {
            var mockCommitmentRespository = new Mock<ICommitmentRepository>();
            mockCommitmentRespository.Setup(x => x.GetByEmployer(It.IsAny<long>())).Returns(Task.FromResult(commitmentsFromRepository));

            var handler = new GetEmployerCommitmentsQueryHandler(mockCommitmentRespository.Object, new GetEmployerCommitmentsValidator());

            var response = await handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 123 });

            response.Commitments.Should().BeSameAs(commitmentsFromRepository);
        }

        [Test]
        public async Task ThenShouldSetHasErrorIndicatorOnResponseIfValidationFails()
        {
            var mockCommitmentRespository = new Mock<ICommitmentRepository>();
            var handler = new GetEmployerCommitmentsQueryHandler(mockCommitmentRespository.Object, new GetEmployerCommitmentsValidator());

            var response = await handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 0 }); // 0 will fail validation

            response.HasError.Should().BeTrue();
        }
    }
}
