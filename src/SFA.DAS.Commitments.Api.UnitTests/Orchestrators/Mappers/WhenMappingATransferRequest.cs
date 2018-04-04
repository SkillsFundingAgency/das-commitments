using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingATransferRequest
    {
        private TransferRequestMapper _mapper;
        private IList<Domain.Entities.TransferRequestSummary> _source;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();
            _source = fixture.Create<IList<Domain.Entities.TransferRequestSummary>>();
            _mapper = new TransferRequestMapper();
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

            result.TransferRequestId.Should().Be(_source[0].TransferRequestId);
            result.ReceivingEmployerAccountId.Should().Be(_source[0].ReceivingEmployerAccountId);
            result.CommitmentId.Should().Be(_source[0].CommitmentId);
            result.SendingEmployerAccountId.Should().Be(_source[0].SendingEmployerAccountId);
            result.Status.Should().Be((TransferApprovalStatus)_source[0].Status);
            result.ApprovedOrRejectedByUserName.Should().Be(_source[0].ApprovedOrRejectedByUserName);
            result.ApprovedOrRejectedByUserEmail.Should().Be(_source[0].ApprovedOrRejectedByUserEmail);
            result.ApprovedOrRejectedOn.Should().Be(_source[0].ApprovedOrRejectedOn);

        }
    }
}
