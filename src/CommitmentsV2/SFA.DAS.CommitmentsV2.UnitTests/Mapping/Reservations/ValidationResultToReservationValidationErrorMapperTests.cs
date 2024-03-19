using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Reservations;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Reservations
{
    [TestFixture]
    public class ValidationResultToReservationValidationErrorMapperTests
    {
        private Fixture _autoFixture;
        private ReservationValidationResult _source;
        private ValidationResultToReservationValidationErrorMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _autoFixture = new Fixture();
            _source = _autoFixture.Create<ReservationValidationResult>();

            _mapper = new ValidationResultToReservationValidationErrorMapper();
        }

        [Test]
        public async Task Map_Errors_ShouldBeSet()
        {
            var result = await _mapper.Map(_source);

            Assert.That(result.ValidationErrors.Length, Is.EqualTo(_source.ValidationErrors.Length));
            foreach (var error in result.ValidationErrors)
            {
                Assert.That(result.ValidationErrors.Any(x =>
                    x.PropertyName == error.PropertyName && x.Reason == error.Reason), Is.True);
            }
        }
    }
}
