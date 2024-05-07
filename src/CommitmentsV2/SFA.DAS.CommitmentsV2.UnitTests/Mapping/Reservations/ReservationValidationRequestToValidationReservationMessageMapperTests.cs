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
        public async Task Map_StartDate_ShouldBeSet()
        {
            var result = await _mapper.Map(_source);
            Assert.That(result.StartDate, Is.EqualTo(_source.StartDate));
        }

        [Test]
        public async Task Map_CourseCode_ShouldBeSet()
        {
            var result = await _mapper.Map(_source);
            Assert.That(result.CourseCode, Is.EqualTo(_source.CourseCode));
        }

        [Test]
        public async Task Map_ReservationId_ShouldBeSet()
        {
            var result = await _mapper.Map(_source);
            Assert.That(result.ReservationId, Is.EqualTo(_source.ReservationId));
        }
    }
}
