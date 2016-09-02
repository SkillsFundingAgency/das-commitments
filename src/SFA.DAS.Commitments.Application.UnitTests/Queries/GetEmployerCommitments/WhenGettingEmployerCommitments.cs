﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Domain;
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

        [Test, AutoData]
        public async Task ThenShouldReturnListOfCommitmentsInResponse(IList<Commitment> commitmentsFromRepository)
        {
            _mockCommitmentRespository.Setup(x => x.GetByEmployer(It.IsAny<long>())).ReturnsAsync(commitmentsFromRepository);

            var response = await _handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 123 });

            response.Data.Should().HaveSameCount(commitmentsFromRepository);
            commitmentsFromRepository.Should().OnlyContain(x => response.Data.Any(y => y.Id == x.Id && y.Name == x.Name));
        }

        [Test]
        public void ThenShouldThrowInvalidRequestExceptionIfValidationFails()
        {
            Func<Task> act = async () => await _handler.Handle(new GetEmployerCommitmentsRequest { AccountId = 0 });

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
