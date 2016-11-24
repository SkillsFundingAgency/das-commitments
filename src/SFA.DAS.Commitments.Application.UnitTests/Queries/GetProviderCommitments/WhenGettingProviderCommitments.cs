using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetProviderCommitments
{
    using SFA.DAS.Commitments.Application.Rules;

    [TestFixture]
    public class WhenGettingProviderCommitments
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetCommitmentsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetCommitmentsQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentsValidator(), new CommitmentRules());
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 124
                }
            });

            _mockCommitmentRespository.Verify(x => x.GetByProvider(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenShouldReturnListOfOnlyActiveCommitmentsInResponse(IList<CommitmentSummary> commitmentsFromRepository)
        {
            var activeCommitments = commitmentsFromRepository.Where(x => x.CommitmentStatus == CommitmentStatus.Active).ToList();
            _mockCommitmentRespository.Setup(x => x.GetByProvider(It.IsAny<long>())).ReturnsAsync(activeCommitments);

            var response = await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 123
                }
            });
            
            response.Data.Count.Should().Be(activeCommitments.Count());
            response.Data.Should().OnlyContain(x => commitmentsFromRepository.Any(y => y.Id == x.Id && y.Reference == x.Reference));
        }

        [Test]
        public void ThenShouldThrowInvalidRequestExceptionIfValidationFails()
        {
            Func<Task> act = async () => await _handler.Handle(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 0
                }
            });

            act.ShouldThrow<ValidationException>();
        }
    }
}
