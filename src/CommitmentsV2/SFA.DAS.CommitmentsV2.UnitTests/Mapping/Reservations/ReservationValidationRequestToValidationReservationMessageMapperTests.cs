using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Mapping.Reservations;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Reservations
{
    [TestFixture]
    public class ReservationValidationRequestToValidationReservationMessageMapperTests
    {
        private Fixture _autoFixture;
        private ReservationValidationRequest _source;
        private ReservationValidationRequestToValidationReservationMessageMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _autoFixture = new Fixture();
            _source = _autoFixture.Create<ReservationValidationRequest>();

            _mapper = new ReservationValidationRequestToValidationReservationMessageMapper();
        }

        [Test]
        public void Map_StartDate_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.StartDate, result.StartDate);
        }

        [Test]
        public void Map_CourseCode_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.CourseCode, result.CourseCode);
        }

        [Test]
        public void Map_AccountId_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.AccountId, result.AccountId);
        }

        [Test]
        public void Map_AccountLegalEntityPublicHashedId_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.AccountLegalEntityPublicHashedId, result.AccountLegalEntityPublicHashedId);
        }

        [Test]
        public void Map_ProviderId_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.ProviderId, result.ProviderId);
        }

        [Test]
        public void Map_ReservationId_ShouldBeSet()
        {
            var result = _mapper.Map(_source);
            Assert.AreEqual(_source.ReservationId, result.ReservationId);
        }
    }
}
