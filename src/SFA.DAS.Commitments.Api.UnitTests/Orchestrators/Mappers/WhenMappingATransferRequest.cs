using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.HashingService;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingATransferRequest
    {
        private TransferRequestMapper _mapper;
        private IList<Domain.Entities.TransferRequestSummary> _source;
        private Mock<IHashingService> _hashingService;

        [SetUp]
        public void Setup()
        {
            _hashingService = new Mock<IHashingService>();
            _hashingService.Setup(x => x.HashValue(It.IsAny<long>())).Returns((long param) => param.ToString());

            var fixture = new Fixture();
            _source = fixture.Create<IList<Domain.Entities.TransferRequestSummary>>();
            _mapper = new TransferRequestMapper(_hashingService.Object);
        }

        [Test]
        public void ThenMappingTheListReturnsTheCorrectCount()
        {
            var result = _mapper.MapFrom(_source);

            result.Count().Should().Be(_source.Count);

        }

        [Test]
        public void ThenMappingToNewObjectMatches()
        {
            var result = _mapper.MapFrom(_source[0]);

            result.HashedTransferRequestId.Should().Be(_source[0].TransferRequestId.ToString());
            result.HashedReceivingEmployerAccountId.Should().Be(_source[0].ReceivingEmployerAccountId.ToString());
            result.HashedCohortRef.Should().Be(_source[0].CommitmentId.ToString());
            result.HashedSendingEmployerAccountId.Should().Be(_source[0].SendingEmployerAccountId.ToString());
            result.Status.Should().Be((TransferApprovalStatus)_source[0].Status);
            result.ApprovedOrRejectedByUserName.Should().Be(_source[0].ApprovedOrRejectedByUserName);
            result.ApprovedOrRejectedByUserEmail.Should().Be(_source[0].ApprovedOrRejectedByUserEmail);
            result.ApprovedOrRejectedOn.Should().Be(_source[0].ApprovedOrRejectedOn);

        }
    }
}
