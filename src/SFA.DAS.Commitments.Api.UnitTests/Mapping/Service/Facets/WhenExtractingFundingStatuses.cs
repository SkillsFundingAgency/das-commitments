using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Mapping.Service.Facets
{
    [TestFixture]
    public class WhenExtractingFundingStatuses
    {
        private FacetMapper _sut;
        private List<Apprenticeship> _data;
        private ApprenticeshipSearchQuery _userQuery;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Apprenticeship>();

            _userQuery = new ApprenticeshipSearchQuery();
            _sut = new FacetMapper(new Mock<ICurrentDateTime>().Object);
        }

        [TestCase(Originator.Employer)]
        [TestCase(Originator.Provider)]
        public void ShouldHaveTransferFundedFacetIfAnyApprenticeshipIsTransferFunded(Originator originator)
        {
            _data.Add(new Apprenticeship
            {
                TransferSenderId = 123L
            });

            var result = _sut.BuildFacets(_data, _userQuery, originator);

            Assert.IsNotNull(result.FundingStatuses);
            Assert.AreEqual(1, result.FundingStatuses.Count);
            Assert.AreEqual(FundingStatus.TransferFunded, result.FundingStatuses[0].Data);
        }

        [TestCase(Originator.Employer)]
        [TestCase(Originator.Provider)]
        public void ShouldNotHaveTransferFundedFacetIfNoApprenticeshipsAreTransferFunded(Originator originator)
        {
            _data.Add(new Apprenticeship());

            var result = _sut.BuildFacets(_data, _userQuery, originator);

            Assert.IsNotNull(result.FundingStatuses);
            Assert.AreEqual(0, result.FundingStatuses.Count);
        }

        [TestCase(Originator.Employer)]
        [TestCase(Originator.Provider)]
        public void ShouldHaveTransferFundedSelectedIfQueryContainsTransferFunded(Originator originator)
        {
            _data.Add(new Apprenticeship
            {
                TransferSenderId = 123L
            });

            _userQuery.FundingStatuses = new List<FundingStatus> {FundingStatus.TransferFunded};

            var result = _sut.BuildFacets(_data, _userQuery, originator);

            Assert.IsNotNull(result.FundingStatuses);
            Assert.AreEqual(1, result.FundingStatuses.Count);
            Assert.AreEqual(true, result.FundingStatuses[0].Selected);
        }

        [TestCase(Originator.Employer)]
        [TestCase(Originator.Provider)]
        public void ShouldNotHaveTransferFundedSelectedIfQueryDoesntContainsTransferFunded(Originator originator)
        {
            _data.Add(new Apprenticeship
            {
                TransferSenderId = 123L
            });

            _userQuery.FundingStatuses = new List<FundingStatus>();

            var result = _sut.BuildFacets(_data, _userQuery, originator);

            Assert.IsNotNull(result.FundingStatuses);
            Assert.AreEqual(1, result.FundingStatuses.Count);
            Assert.AreEqual(false, result.FundingStatuses[0].Selected);
        }
    }
}
