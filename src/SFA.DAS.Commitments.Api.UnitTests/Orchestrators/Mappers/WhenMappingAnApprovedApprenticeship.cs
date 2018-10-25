using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingAnApprovedApprenticeship
    {
        private ApprovedApprenticeshipMapper _mapper;
        private ApprovedApprenticeship _source;

        private Mock<IDataLockMapper> _dataLockMapper;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;

        [SetUp]
        public void Setup()
        {
            _dataLockMapper = new Mock<IDataLockMapper>();
            _dataLockMapper.Setup(x => x.Map(It.IsAny<DataLockStatus>())).Returns(new Types.DataLock.DataLockStatus());

            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _apprenticeshipMapper.Setup(x => x.MapPriceHistory(It.IsAny<PriceHistory>()))
                .Returns(new Types.Apprenticeship.PriceHistory());

            var fixture = new Fixture();
            _source = fixture.Create<ApprovedApprenticeship>();
            _mapper = new ApprovedApprenticeshipMapper(_dataLockMapper.Object, _apprenticeshipMapper.Object);
        }

        [Test]
        public void ThenMapsApprenticeshipValuesCorrectly()
        {
            var result = _mapper.Map(TestHelper.Clone(_source));

            Assert.AreEqual(_source.Id, result.Id);
            Assert.AreEqual(_source.ULN, result.ULN);
            Assert.AreEqual(_source.FirstName, result.FirstName);
            Assert.AreEqual(_source.LastName, result.LastName);
            Assert.AreEqual(_source.DateOfBirth, result.DateOfBirth);
            Assert.AreEqual(_source.TrainingCode, result.TrainingCode);
            Assert.AreEqual(_source.TrainingName, result.TrainingName);
            Assert.AreEqual(_source.StartDate, result.StartDate);
            Assert.AreEqual(_source.EndDate, result.EndDate);
            Assert.AreEqual(_source.StopDate, result.StopDate);
            Assert.AreEqual(_source.ProviderRef, result.ProviderRef);
            Assert.AreEqual(_source.EmployerRef, result.EmployerRef);
            Assert.AreEqual(_source.EmployerAccountId, result.EmployerAccountId);
            Assert.AreEqual(_source.LegalEntityName, result.LegalEntityName);
            Assert.AreEqual(_source.ProviderId, result.ProviderId);
            Assert.AreEqual(_source.ProviderName, result.ProviderName);
            Assert.AreEqual(_source.TransferSenderId, result.TransferSenderId);
            Assert.AreEqual(_source.CohortReference, result.CohortReference);
            Assert.AreEqual(_source.PaymentOrder, result.PaymentOrder);
        }

        [Test]
        public void ThenDataLocksAreMappedUsingTheDataLockMapper()
        {
            _mapper.Map(TestHelper.Clone(_source));
            _dataLockMapper.Verify(x => x.Map(It.IsAny<DataLockStatus>()), Times.Exactly(_source.DataLocks.Count));
        }

        [Test]
        public void ThenPriceEpisodesAreMappedUsingTheApprenticeshipMapper()
        {
            _mapper.Map(TestHelper.Clone(_source));
            _apprenticeshipMapper.Verify(x => x.MapPriceHistory(It.IsAny<PriceHistory>()), Times.Exactly(_source.PriceEpisodes.Count));
        }
    }
}
