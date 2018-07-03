using NUnit.Framework;
using FluentAssertions;
using Moq;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.HashingService;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using AutoFixture;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.Commitments.Application.Rules;
using System.Collections.Generic;
using SFA.DAS.Commitments.Support.SubSite.Enums;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Mappers
{
    [TestFixture]
    public class CommitmentMapperTest
    {
        private Mock<IHashingService> _hashingService;
        private Mock<ICommitmentStatusCalculator> _statusCalculator;
        private Mock<ICommitmentRules> _commitmentRules;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private const string _hashedId = "HBCDE5";


        private Commitment _mockedCommitment;
        private CommitmentMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _hashingService = new Mock<IHashingService>();
            _statusCalculator = new Mock<ICommitmentStatusCalculator>();
            _commitmentRules = new Mock<ICommitmentRules>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();


            var dataFixture = new Fixture();
            _mockedCommitment = dataFixture.Build<Commitment>().Create();

            _commitmentRules
                .Setup(x => x.DetermineAgreementStatus(It.IsAny<List<Apprenticeship>>()))
                .Returns(AgreementStatus.BothAgreed);

            _statusCalculator
                .Setup(x => x.GetStatus(It.IsAny<EditStatus>(),
                                                     It.IsAny<int>(),
                                                     It.IsAny<LastAction>(),
                                                     It.IsAny<AgreementStatus?>(),
                                                     It.IsAny<long?>(),
                                                     It.IsAny< TransferApprovalStatus?>()))
                  .Returns(RequestStatus.Approved);

            _hashingService
             .Setup(o => o.HashValue(It.IsAny<long>()))
             .Returns(_hashedId);

            _mapper = new CommitmentMapper(_hashingService.Object,
                _statusCalculator.Object,
                _commitmentRules.Object,
                _apprenticeshipMapper.Object);

        }

        [Test]
        public void ShouldCallService()
        {
            var result = _mapper.MapToCommitmentSummaryViewModel(_mockedCommitment);

            _commitmentRules.Verify(x => x.DetermineAgreementStatus(It.IsAny<List<Apprenticeship>>()), Times.AtLeastOnce);

            _statusCalculator.Verify(x => x.GetStatus(It.IsAny<EditStatus>(),
                                                      It.IsAny<int>(),
                                                     It.IsAny<LastAction>(),
                                                     It.IsAny<AgreementStatus?>(),
                                                     It.IsAny<long?>(),
                                                     It.IsAny<TransferApprovalStatus?>()), Times.AtLeastOnce);

            _hashingService.Verify(o => o.HashValue(It.IsAny<long>()), Times.AtLeastOnce);

        }

        [Test]
        public void ShouldMapToVaLidCommitmentSummaryViewModel()
        {
            var result = _mapper.MapToCommitmentSummaryViewModel(_mockedCommitment);

            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();
            result.CohortReference.Should().BeSameAs(_hashedId);
        }

        [Test]
        public void ShouldMapToValidCommitmentDetailViewModel()
        {
            var result = _mapper.MapToCommitmentDetailViewModel(_mockedCommitment);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentDetailViewModel>();
            result.CommitmentApprenticeships.Should().NotBeNull();
            result.CommitmentSummary.Should().NotBeNull();
        }

    }
}
