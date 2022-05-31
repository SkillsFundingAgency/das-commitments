using NUnit.Framework;
using FluentAssertions;
using Moq;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using AutoFixture;
using SFA.DAS.Commitments.Support.SubSite.Services;
using System.Collections.Generic;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Mappers
{
    [TestFixture]
    public class CommitmentMapperTest
    {
        private Mock<IEncodingService> _encodingService;
        private Mock<ICommitmentStatusCalculator> _statusCalculator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        private const string _hashedId = "HBCDE5";

        private GetSupportCohortSummaryQueryResult _mockedCommitmentResult;
        private GetSupportApprenticeshipQueryResult _mockedSupportApprenticeshipResult;
        private CommitmentMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _encodingService = new Mock<IEncodingService>();
            _statusCalculator = new Mock<ICommitmentStatusCalculator>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();

            var dataFixture = new Fixture();
            _mockedCommitmentResult = dataFixture.Build<GetSupportCohortSummaryQueryResult>().Create();
            _mockedSupportApprenticeshipResult = dataFixture.Build<GetSupportApprenticeshipQueryResult>().Create();

            _statusCalculator
                .Setup(x => x.GetStatus(It.IsAny<EditStatus>(),
                                                     It.IsAny<int>(),
                                                     It.IsAny<LastAction>(),
                                                     It.IsAny<AgreementStatus?>(),
                                                     It.IsAny<long?>(),
                                                     It.IsAny<TransferApprovalStatus?>()))
                  .Returns(RequestStatus.Approved);

            _encodingService
             .Setup(o => o.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()))
             .Returns(_hashedId);

            _mapper = new CommitmentMapper(_encodingService.Object,
                _statusCalculator.Object,
                _apprenticeshipMapper.Object);
        }

        [Test]
        public void ShouldCallService()
        {
            var result = _mapper.MapToCommitmentSummaryViewModel(_mockedCommitmentResult, _mockedSupportApprenticeshipResult);

            _statusCalculator.Verify(x => x.GetStatus(It.IsAny<EditStatus>(),
                                                      It.IsAny<int>(),
                                                     It.IsAny<LastAction>(),
                                                     It.IsAny<AgreementStatus?>(),
                                                     It.IsAny<long?>(),
                                                     It.IsAny<TransferApprovalStatus?>()), Times.AtLeastOnce);

            _encodingService.Verify(o => o.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()), Times.AtLeastOnce);
        }

        [Test]
        public void ShouldMapToVaLidCommitmentSummaryViewModel()
        {
            var result = _mapper.MapToCommitmentSummaryViewModel(_mockedCommitmentResult, _mockedSupportApprenticeshipResult);

            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentSummaryViewModel>();
            result.CohortReference.Should().BeSameAs(_hashedId);
        }

        [Test]
        public void ShouldMapToValidCommitmentDetailViewModel()
        {
            var result = _mapper.MapToCommitmentDetailViewModel(_mockedCommitmentResult, _mockedSupportApprenticeshipResult);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommitmentDetailViewModel>();
            result.CommitmentApprenticeships.Should().NotBeNull();
            result.CommitmentSummary.Should().NotBeNull();
        }
    }
}