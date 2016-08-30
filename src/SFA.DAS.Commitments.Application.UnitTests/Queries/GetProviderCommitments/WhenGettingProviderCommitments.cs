using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetProviderCommitments
{
    [TestFixture]
    public class WhenGettingProviderCommitments
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetProviderCommitmentsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetProviderCommitmentsQueryHandler(_mockCommitmentRespository.Object, new GetProviderCommitmentsValidator());
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetProviderCommitmentsRequest { ProviderId = 124 });

            _mockCommitmentRespository.Verify(x => x.GetByProvider(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenShouldReturnListOfCommitmentsInResponse(IList<Commitment> commitmentsFromRepository)
        {
            _mockCommitmentRespository.Setup(x => x.GetByProvider(It.IsAny<long>())).Returns(Task.FromResult(commitmentsFromRepository));

            var response = await _handler.Handle(new GetProviderCommitmentsRequest { ProviderId = 123 });

            response.Data.Should().HaveSameCount(commitmentsFromRepository);
            commitmentsFromRepository.Should().OnlyContain(x => response.Data.Any(y => y.Id == x.Id && y.Name == x.Name));
        }

        [Test]
        public void ThenShouldThrowInvalidRequestExceptionIfValidationFails()
        {
            Func<Task> act = async () => await _handler.Handle(new GetProviderCommitmentsRequest { ProviderId = 0 });

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
